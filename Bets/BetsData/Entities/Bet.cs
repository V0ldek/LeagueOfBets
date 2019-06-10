using System;
using System.Collections.Generic;
using System.Text;

namespace BetsData.Entities
{
    public class Bet
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public Stake Stake { get; set; }

        public int StakeId { get; set; }

        public uint Amount { get; set; }
    }
}
