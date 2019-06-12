using System;
using System.Collections.Generic;

namespace LeagueOfBets.Models
{
    public class MatchViewModel
    {
        public int Id { get; }

        public string BlueTeamName { get; }

        public string RedTeamName { get; }

        public int BlueScore { get; }

        public int RedScore { get; }

        public int BestOf { get; }

        public bool IsFinished => BlueScore == BestOf || RedScore == BestOf;

        public string BlueTeamLogoUrl { get; }

        public string RedTeamLogoUrl { get; }

        public DateTime StartDateTime { get; }

        public MatchViewModel(
            int id,
            string blueTeamName,
            string redTeamName,
            int blueScore,
            int redScore,
            int bestOf,
            string blueTeamLogoUrl,
            string redTeamLogoUrl,
            DateTime startDateTime)
        {
            Id = id;
            BlueTeamName = blueTeamName;
            RedTeamName = redTeamName;
            BlueScore = blueScore;
            RedScore = redScore;
            BestOf = bestOf;
            BlueTeamLogoUrl = blueTeamLogoUrl;
            RedTeamLogoUrl = redTeamLogoUrl;
            StartDateTime = startDateTime;
        }
    }
}