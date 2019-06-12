using System;
using System.Collections.Generic;
using System.Text;

namespace BetsMatchUpdateWorker
{
    public class MatchUpdate
    {
        public int Id { get; set; }

        public int BestOf { get; set; }

        public int BlueScore { get; set; }

        public int RedScore { get; set; }

        public int OldBlueScore { get; set; }

        public int OldRedScore { get; set; }

        public bool IsFinished => BlueScore == BestOf || RedScore == BestOf;
    }
}
