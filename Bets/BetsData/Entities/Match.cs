using System.Collections.Generic;
using BetsData.Entities.Enums;

namespace BetsData.Entities
{
    public class Match
    {
        public int Id { get; set; }

        public int BlueScore { get; set; }

        public int RedScore { get; set; }

        public int BestOf { get; set; }

        public Side? WinningSide
        {
            get
            {
                if (BlueScore == BestOf)
                {
                    return Side.Blue;
                }
                else if (RedScore == BestOf)
                {
                    return Side.Red;
                }

                return null;
            }
        }

        public bool IsFinished => WinningSide != null;

        public ICollection<Stake> Stakes { get; set; }
    }
}
