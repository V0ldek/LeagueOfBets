using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MatchesData;
using MatchesData.Entities;
using MatchesWorker.ApiClient;
using MatchesWorker.Cron;
using MatchesWorker.EventQueue;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Newtonsoft.Json;

namespace MatchesWorker
{
    public sealed class Program : IDisposable
    {
        private readonly IApiClient _apiClient;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CronScheduler _cronScheduler;
        private readonly ILogger _logger;
        private readonly MatchesConfiguration _matchesConfiguration;
        private readonly IMatchEventProducer _matchEventProducer;

        public Program(ILogger logger)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, args) =>
            {
                _cancellationTokenSource.Cancel();
                args.Cancel = true;
            };

            var configuration = GetConfiguration();
            _cronScheduler = new CronScheduler(GetConfigurationSection<CronConfiguration>(configuration));
            _apiClient = new PandaScoreApiClient(GetConfigurationSection<PandaScoreConfiguration>(configuration));
            _matchesConfiguration = GetConfigurationSection<MatchesConfiguration>(configuration);
            _matchEventProducer = new RabbitMqMatchEventProducer("leagueofbets_bets_event_queue");
            _logger = logger;
        }

        public void Dispose() => _cancellationTokenSource?.Dispose();

        public static async Task Main(string[] args)
        {
            var logger = new LoggerFactory().AddConsole().CreateLogger("MatchesWorker");

            try
            {
                using (var program = new Program(logger))
                {
                    await program.RunAsync();
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message.AddTimestamp());
            }
        }

        public async Task RunAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                _logger.LogInformation("Starting cron worker.".AddTimestamp());
                try
                {
                    await _cronScheduler.RunAsync(JobAsync, _cancellationTokenSource.Token);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, exception.Message.AddTimestamp());
                }
            }
        }

        private async Task JobAsync()
        {
            _logger.LogInformation("Executing job.".AddTimestamp());
            using (var dbContext = new MatchesDbContext(_matchesConfiguration.ConnectionString))
            {
                await FetchNewMatchesAsync(dbContext);
                DetachAll(dbContext);
                await UpdateMatchesAsync(dbContext);
            }
        }

        private async Task FetchNewMatchesAsync(MatchesDbContext dbContext)
        {
            DateTime fetchSince;
            if (!await dbContext.Matches.AnyAsync())
            {
                fetchSince = _matchesConfiguration.FetchSince;
            }
            else
            {
                fetchSince = await dbContext.Matches.MaxAsync(m => m.StartDateTime) + TimeSpan.FromDays(1);
            }

            var fetchTo = DateTime.Now + TimeSpan.FromDays(_matchesConfiguration.FetchAheadInDays);

            _logger.LogInformation($"Fetching new matches between {fetchSince:yyyy-MM-dd} and {fetchTo:yyyy-MM-dd}".AddTimestamp());

            var matches = (await _apiClient.FetchMatchesBetweenAsync(fetchSince, fetchTo)).ToList();

            foreach (var match in matches)
            {
                _logger.LogInformation($"New match:\n {JsonConvert.SerializeObject(match)}".AddTimestamp());
            }

            dbContext.Matches.AddRange(matches);

            foreach (var match in matches)
            {
                _matchEventProducer.PublishNewMatch(match);
            }

            await dbContext.SaveChangesAsync();
        }

        private async Task UpdateMatchesAsync(MatchesDbContext dbContext)
        {
            if (await dbContext.Matches.AllAsync(Match.IsFinishedExpression))
            {
                return;
            }

            var updateSince = await dbContext.Matches
                .Where(Match.IsNotFinishedExpression)
                .MinAsync(m => m.StartDateTime);
            var updateTo = DateTime.Now;

            if (updateTo < updateSince)
            {
                _logger.LogInformation("Skipping update.".AddTimestamp());
                return;
            }

            _logger.LogInformation($"Fetching updates between {updateSince:yyyy-MM-dd} and {updateTo:yyyy-MM-dd}".AddTimestamp());

            var matches = (await _apiClient.FetchMatchesBetweenAsync(updateSince, updateTo)).ToList();

            foreach (var match in matches)
            {
                _logger.LogInformation($"Updated match:\n {JsonConvert.SerializeObject(match)}".AddTimestamp());
            }

            dbContext.UpdateRange(matches);

            foreach (var match in matches)
            {
                if (match.IsFinished)
                {
                    _matchEventProducer.PublishMatchUpdate(match);
                }
            }

            await dbContext.SaveChangesAsync();
        }


        private static void DetachAll(DbContext dbContext)
        {
            foreach (var match in dbContext.ChangeTracker.Entries().ToList())
            {
                if (match.Entity != null)
                {
                    match.State = EntityState.Detached;
                }
            }
        }

        private static IConfigurationRoot GetConfiguration() =>
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();

        private static T GetConfigurationSection<T>(IConfiguration configuration) where T : new()
        {
            var section = new T();
            configuration.GetSection(typeof(T).Name).Bind(section);
            return section;
        }
    }
}