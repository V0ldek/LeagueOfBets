using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BetsData;
using BetsData.Entities;
using BetsNewBetWorker.EventQueue;
using BetsWorkerBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BetsNewBetWorker
{
    public sealed class NewBetWorker : Worker<Bet>
    {
        private readonly ILogger _logger;
        private readonly BetsConfiguration _configuration;

        private List<Bet> _betsBatch = new List<Bet>();
        private ulong _lastDeliveryTag;

        public NewBetWorker(ILogger logger) : base(new RabbitMqMultipleAckObjectEventConsumer<Bet>("leagueofbets_bets_event_queue", logger), logger)
        {
            var configuration = GetConfiguration();
            _configuration = GetConfigurationSection<BetsConfiguration>(configuration);
            _logger = logger;
            EventConsumer.PrefetchLimit = _configuration.PrefetchLimit;
        }

        public static async Task Main(string[] args)
        {
            var logger = new LoggerFactory().AddConsole().CreateLogger(nameof(NewBetWorker));

            try
            {
                using (var worker = new NewBetWorker(logger))
                {
                    await worker.RunAsync();
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message.AddTimestamp());
            }
        }

        public override async Task RunAsync()
        {
            var batchProcessingTask = ProcessBatchesAsync(CancellationTokenSource.Token);

            await base.RunAsync();
            await batchProcessingTask;
        }

        protected override void HandleEvent(Bet bet, ulong deliveryTag)
        {
            _logger.LogInformation($"New bet for stake {bet.StakeId}");
            lock (_betsBatch)
            {
                _betsBatch.Add(bet);
                _lastDeliveryTag = deliveryTag;
            }
        }

        private async Task ProcessBatchesAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Sleeping for {_configuration.BatchFrequencyInMilliseconds} ms.".AddTimestamp());
                await Task.Delay(_configuration.BatchFrequencyInMilliseconds, cancellationToken);

                List<Bet> batch;
                ulong deliveryTag;
                lock (_betsBatch)
                {
                    batch = _betsBatch;
                    _betsBatch = new List<Bet>();
                    deliveryTag = _lastDeliveryTag;
                }

                if (!batch.Any())
                {
                    _logger.LogInformation("No batch to process.".AddTimestamp());
                    continue;
                }

                _logger.LogInformation($"Processing batch of {batch.Count} bets.".AddTimestamp());

                var stakeIds = batch.Select(b => b.StakeId);
                var betsByStakeIds = batch.GroupBy(b => b.StakeId).ToDictionary(g => g.Key, g => g.Count());

                using (var dbContext = new BetsDbContext(_configuration.ConnectionString))
                using (var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken))
                {
                    var matches = await dbContext.Matches.Include(m => m.Stakes).Where(m => m.Stakes.Any(s => stakeIds.Contains(s.Id)))
                        .ToListAsync(cancellationToken);
                    _logger.LogInformation($"Got {matches.Count} matches".AddTimestamp());

                    var newStakes = matches.SelectMany(m => m.Stakes.Where(s => s.IsBettable).Select(s => CalculateNewStake(s, m, betsByStakeIds))).ToList();

                    foreach (var oldStake in matches.SelectMany(m => m.Stakes.Where(s => s.IsBettable)))
                    {
                        oldStake.IsBettable = false;
                    }
                    _logger.LogInformation("Calculated stakes.");
                    dbContext.Stakes.AddRange(newStakes);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    transaction.Commit();
                }

                EventConsumer.Ack(deliveryTag);
            }
        }

        private Stake CalculateNewStake(Stake stake, Match match, IDictionary<int, int> betsByStakeIds)
        {
            var newStake = new Stake
            {
                BlueScore = stake.BlueScore,
                RedScore = stake.RedScore,
                IsBettable = true,
                MatchId = stake.MatchId,
                Ratio = stake.Ratio
            };

            if (betsByStakeIds.ContainsKey(stake.Id))
            {
                _logger.LogInformation($"Penalty = {_configuration.BaseStep * _configuration.BetPenaltyRatio * betsByStakeIds[stake.Id]}");
                _logger.LogInformation($"Base = {_configuration.BaseStep}");
                _logger.LogInformation($"BetRatio = {_configuration.BetPenaltyRatio}");
                _logger.LogInformation($"Bets = {betsByStakeIds[stake.Id]}");
                newStake.Ratio -= _configuration.BaseStep * _configuration.BetPenaltyRatio * betsByStakeIds[stake.Id];
            }

            foreach (var otherStake in match.Stakes)
            {
                if (stake == otherStake || !betsByStakeIds.ContainsKey(otherStake.Id))
                {
                    continue;
                }

                var blueDifference = stake.BlueScore - otherStake.BlueScore;
                var redDifference = stake.RedScore - otherStake.RedScore;
                var distance = blueDifference * blueDifference + redDifference * redDifference;
                var increase = _configuration.BaseStep * distance *
                    _configuration.DistanceRatio * betsByStakeIds[otherStake.Id];
                newStake.Ratio += increase;

                _logger.LogInformation($"Distance = {distance}");
                _logger.LogInformation($"DistanceRatio = {_configuration.DistanceRatio}");
                _logger.LogInformation($"Increase = {increase}");
            }

            newStake.Ratio = Math.Clamp(newStake.Ratio, _configuration.MinimalStake, _configuration.MaximalStake);
            return newStake;
        }
    }
}
