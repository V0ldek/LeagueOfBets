using System.Linq;
using System.Security.Claims;

namespace BetsAPI.Controllers
{
    internal static class ClaimsPrincipalExtensions
    {
        public static string GetSubjectClaim(this ClaimsPrincipal claimsPrincipal) =>
            claimsPrincipal.Claims
                .SingleOrDefault(
                    c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
    }
}
