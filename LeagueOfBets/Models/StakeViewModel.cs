using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeagueOfBets.Models
{
    public class StakeViewModel
    {
        public int Id { get; }

        public int MatchId { get; }

        public int BlueScore { get; }

        public int RedScore { get; }

        public float Ratio { get; }

        public StakeViewModel(int id, int matchId, int blueScore, int redScore, float ratio)
        {
            Id = id;
            MatchId = matchId;
            BlueScore = blueScore;
            RedScore = redScore;
            Ratio = ratio;
        }
    }
}