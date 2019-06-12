namespace BetsNewBetWorker
{
    public class BetsConfiguration
    {
        public string ConnectionString { get; set; }

        public ushort PrefetchLimit { get; set; }

        public int BatchFrequencyInMilliseconds { get; set; }

        public float MinimalStake { get; set; }

        public float MaximalStake { get; set; }

        public float BaseDistanceStep { get; set; }

        public float DistanceRatio { get; set; }

        public float BetPenaltyRatio { get; set; }
    }
}
