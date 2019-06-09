using System;
using System.Collections.Generic;
using MatchesData.Entities.Enums;

namespace MatchesData.Entities
{
    public class Match
    {
        public int Id { get; set; }

        public DateTime StartDateTime { get; set; }

        public GameFormat Format { get; set; }

        public int BlueScore { get; set; }

        public int RedScore { get; set; }

        public bool IsFinished { get; set; }

        public ICollection<MatchParticipation> Participations { get; set; } = new HashSet<MatchParticipation>();
    }
}