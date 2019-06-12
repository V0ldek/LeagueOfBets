using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MatchesData.Entities.Enums;

namespace MatchesData.Entities
{
    public class Match
    {
        public int Id { get; set; }

        public DateTime StartDateTime { get; set; }

        public int BestOf { get; set; }

        public int BlueScore { get; set; }

        public int RedScore { get; set; }

        public bool IsFinished => BlueScore == BestOf || RedScore == BestOf;

        public static Expression<Func<Match, bool>> IsFinishedExpression => m => m.BlueScore == m.BestOf || m.RedScore == m.BestOf;

        public static Expression<Func<Match, bool>> IsNotFinishedExpression => m => m.BlueScore != m.BestOf && m.RedScore != m.BestOf;

        public ICollection<MatchParticipation> Participations { get; set; } = new HashSet<MatchParticipation>();
    }
}