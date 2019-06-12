using System;
using System.Collections.Generic;
using BetsData.Entities.Enums;

namespace BetsData.Entities
{
    public class Stake
    {
        public int Id { get; set; }

        public Match Match { get; set; }

        public int MatchId { get; set; }

        public int BlueScore { get; set; }

        public int RedScore { get; set; }
        
        public float Ratio { get; set; }

        public DateTime Timestamp { get; set; }

        public ICollection<Bet> Bets { get; set; }
    }
}
