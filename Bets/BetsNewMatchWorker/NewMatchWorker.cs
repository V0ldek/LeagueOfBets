using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BetsData;
using BetsData.Entities;
using BetsWorkerBase;
using BetsWorkerBase.EventQueue;
using Microsoft.Extensions.Logging;

namespace BetsNewMatchWorker
{
    public sealed class NewMatchWorker : Worker<Match>
    {
        private readonly ILogger _logger;
        private readonly BetsConfiguration _configuration;

        public NewMatchWorker(ILogger logger) : base(new RabbitMqObjectEventConsumer<Match>("leagueofbets_bets_event_queue", logger), logger)
        {
            var configuration = GetConfiguration();
            _configuration = GetConfigurationSection<BetsConfiguration>(configuration);
            _logger = logger;
        }

        public static async Task Main(string[] args)
        {
            var logger = new LoggerFactory().AddConsole().CreateLogger(nameof(NewMatchWorker));

            try
            {
                using (var worker = new NewMatchWorker(logger))
                {
                    await worker.RunAsync();
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message.AddTimestamp());
            }
        }

        protected override void HandleEvent(Match match, ulong deliveryTag)
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
                        Ratio = _configuration.DefaultRatio,
                        IsBettable = true
                    });
                stakes.Add(
                    new Stake
                    {
                        BlueScore = losersScore,
                        RedScore = match.BestOf,
                        MatchId = match.Id,
                        Ratio = _configuration.DefaultRatio,
                        IsBettable = true
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
    }
}
