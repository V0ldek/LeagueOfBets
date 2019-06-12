using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BetsData;
using BetsData.Entities;
using BetsData.Entities.Enums;
using BetsWorker.EventQueue;
using MatchesWorker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BetsWorker
{
    public sealed class Program : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger _logger;
        private readonly IMatchEventConsumer _matchEventConsumer;
        private readonly BetsConfiguration _configuration;

        public Program(ILogger logger)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, args) =>
            {
                _cancellationTokenSource.Cancel();
                args.Cancel = true;
            };

            var configuration = GetConfiguration();
            _configuration = GetConfigurationSection<BetsConfiguration>(configuration);
            _logger = logger;
            _matchEventConsumer = new RabbitMqMatchEventConsumer("leagueofbets_bets_event_queue", _logger);
        }

        public static async Task Main(string[] args)
        {
            var logger = new LoggerFactory().AddConsole().CreateLogger("BetsWorker");

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
            _logger.LogInformation("Listening for new matches...".AddTimestamp());
            _matchEventConsumer.NewMatch += HandleNewMatch;
            _matchEventConsumer.BeginConsuming();
            await Task.Delay(-1, _cancellationTokenSource.Token);
            _matchEventConsumer.NewMatch -= HandleNewMatch;
        }

        private void HandleNewMatch(Match match)
        {
            var stakes = new List<Stake>();

            for (var losersScore = 0; losersScore < match.BestOf; ++losersScore)
            {
                stakes.Add(
                    new Stake
                    { 
                        BlueScore = match.BestOf,
                        RedScore = losersScore,
                        MatchId = match.Id,
                        Ratio = 1.05f
                    });
                stakes.Add(
                    new Stake
                    {
                        BlueScore = losersScore,
                        RedScore = match.BestOf,
                        MatchId = match.Id,
                        Ratio = _configuration.DefaultRatio
                    });
            }

            _logger.LogInformation($"Received new match {match.Id}, adding stakes.".AddTimestamp());

            using (var dbContext = new BetsDbContext(_configuration.ConnectionString))
            {
                dbContext.Matches.Add(match);
                dbContext.Stakes.AddRange(stakes);

                dbContext.SaveChanges();
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

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
            _matchEventConsumer?.Dispose();
        }
    }
}
