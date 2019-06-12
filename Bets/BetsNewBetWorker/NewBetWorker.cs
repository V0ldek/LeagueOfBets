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
                await Task.Delay(_configuration.BatchFrequencyInMilliseconds, cancellationToken);

                List<Bet> batch;
                ulong deliveryTag;
                lock (_betsBatch)
                {
                    batch = _betsBatch;
                    _betsBatch = new List<Bet>();
                    if (!_betsBatch.Any())
                    {
                        continue;
                    }

                    deliveryTag = _lastDeliveryTag;
                }

                _logger.LogInformation($"Processing batch of {_betsBatch.Count} bets.");

                var stakeIds = batch.Select(b => b.StakeId);
                var betsByStakeIds = batch.GroupBy(b => b.StakeId).ToDictionary(g => g.Key, g => g.Count());

                using (var dbContext = new BetsDbContext(_configuration.ConnectionString))
                using (var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken))
                {
                    var matches = await dbContext.Matches.Where(m => m.Stakes.Any(s => stakeIds.Contains(s.Id) && s.IsBettable))
                        .ToListAsync(cancellationToken);

                    var newStakes = matches.SelectMany(m => m.Stakes.Select(s => CalculateNewStake(s, m, betsByStakeIds))).ToList();

                    foreach (var oldStake in matches.SelectMany(m => m.Stakes))
                    {
                        oldStake.IsBettable = false;
                    }

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
                newStake.Ratio -= _configuration.BaseDistanceStep * _configuration.BetPenaltyRatio;
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
                var increase = _configuration.BaseDistanceStep * distance *
                    _configuration.DistanceRatio * betsByStakeIds[otherStake.Id];
                newStake.Ratio += increase;
            }

            newStake.Ratio = Math.Clamp(newStake.Ratio, _configuration.MinimalStake, _configuration.MaximalStake);
            return newStake;
        }
    }
}
