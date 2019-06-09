using System;

namespace MatchesWorker
{
    public class MatchesConfiguration
    {
        public DateTime FetchSince { get; set; }

        public string ConnectionString { get; set; }

        public int FetchAheadInDays { get; set; }
    }
}