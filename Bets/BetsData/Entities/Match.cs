using System.Collections.Generic;
using BetsData.Entities.Enums;

namespace BetsData.Entities
{
    public class Match
    {
        public int Id { get; set; }

        public bool IsFinished { get; set; }

        public Side? WinningSide { get; set; }

        public int? LosersScore { get; set; }

        public ICollection<Stake> Stakes { get; set; }
    }
}
