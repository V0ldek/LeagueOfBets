namespace BetsMatchUpdateWorker
{
    public class BetsConfiguration
    {
        public string ConnectionString { get; set; }

        public float BaseRatioStep { get; set; }

        public float MinimalStake { get; set; }

        public float MaximalStake { get; set; }
    }
}
