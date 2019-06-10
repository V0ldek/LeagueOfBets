using System.Linq;
using System.Security.Claims;

namespace BetsAPI.Controllers
{
    internal static class ClaimsPrincipalExtensions
    {
        public static string GetSubjectClaim(this ClaimsPrincipal claimsPrincipal) =>
            claimsPrincipal.Claims.SingleOrDefault(c => c.Type == "sub")?.Value;
    }
}
