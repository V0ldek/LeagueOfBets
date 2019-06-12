using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BetsData;
using BetsData.Entities;
using BetsWorkerBase;
using BetsWorkerBase.EventQueue;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BetsMatchUpdateWorker
{
    public sealed class MatchUpdateWorker : Worker<MatchUpdate>
    {
        private readonly ILogger _logger;
        private readonly BetsConfiguration _configuration;

        public MatchUpdateWorker(ILogger logger) : base(new RabbitMqObjectEventConsumer<MatchUpdate>("leagueofbets_bets_event_queue", logger), logger)
        {
            var configuration = GetConfiguration();
            _configuration = GetConfigurationSection<BetsConfiguration>(configuration);
            _logger = logger;
        }

        public static async Task Main(string[] args)
        {
            var logger = new LoggerFactory().AddConsole().CreateLogger(nameof(MatchUpdateWorker));

            try
            {
                using (var worker = new MatchUpdateWorker(logger))
                {
                    await worker.RunAsync();
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message.AddTimestamp());
            }
        }

        protected override void HandleEvent(MatchUpdate matchUpdate, ulong deliveryTag)
        {
            if (matchUpdate.IsFinished)
            {
                return;
            }

            _logger.LogInformation($"Received matchUpdate update for {matchUpdate.Id}, calculating stakes.".AddTimestamp());

            using (var dbContext = new BetsDbContext(_configuration.ConnectionString))
            {
                using (var transaction = dbContext.Database.BeginTransaction(IsolationLevel.RepeatableRead))
                {
                    var stakes = dbContext.Stakes.Where(s => s.MatchId == matchUpdate.Id && s.IsBettable).ToList();
                    var newStakes = stakes.Select(s => CalculateNewStake(s, matchUpdate));
                    dbContext.Stakes.AddRange(newStakes);

                    var match = dbContext.Matches.Single(m => m.Id == matchUpdate.Id);
                    match.BlueScore = matchUpdate.BlueScore;
                    match.RedScore = matchUpdate.RedScore;

                    dbContext.SaveChanges();
                    transaction.Commit();
                }
            }
        }

        private Stake CalculateNewStake(Stake oldStake, MatchUpdate matchUpdate)
        {
            var newStake = new Stake
            {
                MatchId = oldStake.MatchId,
                BlueScore = oldStake.BlueScore,
                RedScore = oldStake.RedScore,
                IsBettable = false,
                Ratio = oldStake.Ratio
            };

            if (oldStake.BlueScore < matchUpdate.BlueScore || oldStake.RedScore < matchUpdate.RedScore)
            {
                return newStake;
            }

            newStake.IsBettable = true;
            if ((oldStake.BlueScore > oldStake.RedScore && matchUpdate.BlueScore > matchUpdate.OldBlueScore) || 
                (oldStake.RedScore > oldStake.BlueScore && matchUpdate.RedScore > matchUpdate.OldRedScore))
            {
                newStake.Ratio -= _configuration.BaseRatioStep;
            }
            else
            {
                newStake.Ratio += _configuration.BaseRatioStep;
            }

            newStake.Ratio = Math.Clamp(newStake.Ratio, _configuration.MinimalStake, _configuration.MaximalStake);

            return newStake;
        }
    }
}
