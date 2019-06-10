using MatchesData.Entities.Enums;

namespace MatchesData.Entities
{
    public class MatchParticipation
    {
        public Match Match { get; set; }
        public int MatchId { get; set; }

        public Team Team { get; set; }
        public int TeamId { get; set; }

        public Side Side { get; set; }
    }
}