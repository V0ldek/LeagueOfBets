using System;
using System.Collections.Generic;
using System.Text;
using MatchesData.Entities;

namespace MatchesWorker.EventQueue
{
    internal interface IMatchEventProducer
    {
        void PublishNewMatch(Match match);

        void PublishMatchUpdate(Match match);

    }
}
