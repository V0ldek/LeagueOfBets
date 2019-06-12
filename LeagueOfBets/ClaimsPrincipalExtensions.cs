using System.Linq;
using System.Security.Claims;

namespace LeagueOfBets
{
    internal static class ClaimsPrincipalExtensions
    {
        public static string GetSubjectClaim(this ClaimsPrincipal claimsPrincipal) =>
            claimsPrincipal.Claims.SingleOrDefault(c => c.Type == "sub")?.Value;
    }
}