using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace Identity
{
    public static class IdentityConfiguration
    {
        public static IEnumerable<IdentityResource> IdentityResources => new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };

        public static IEnumerable<ApiResource> Apis => new List<ApiResource>
        {
            new ApiResource("matches", "MatchesAPI"),
            new ApiResource("bets", "BetsAPI")
        };

        public static IEnumerable<Client> Clients => new List<Client>
        {
            new Client
            {
                ClientId = "leagueofbets_web",
                ClientName = "LeagueOfBets Web",
                AllowedGrantTypes = GrantTypes.Hybrid,

                ClientSecrets =
                {
                    new Secret("developer".Sha256())
                },
                RedirectUris = { "https://localhost:10443/signin-oidc"},
                PostLogoutRedirectUris = { "https://localhost:10443/signout-callback-oidc"},
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "matches",
                    "bets"
                },
                AllowOfflineAccess = true
            }
        };

        public static List<TestUser> TestUsers => new List<TestUser>
        {
            new TestUser
            {
                SubjectId = "42",
                Username = "v0ldek",
                Password = "developer"
            }
        };
    }
}