using System;
using System.Collections.Generic;
using System.Text;

namespace MatchesData.Entities
{
    public class Team
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string LogoUrl { get; set; }

        public ICollection<MatchParticipation> MatchParticipations { get; set; } = new HashSet<MatchParticipation>();
    }
}
