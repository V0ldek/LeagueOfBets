using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace UsersAPI
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
            new ApiResource("matches", "MatchesAPI")
        };

        public static IEnumerable<Client> Clients => new List<Client>
        {
            new Client
            {
                ClientId = "leagueofbets_web",
                ClientName = "LeagueOfBets Web",
                AllowedGrantTypes = GrantTypes.Implicit,

                RedirectUris = { "http://leaugeofbets_web/signin-oidc"},
                PostLogoutRedirectUris = { "http://leagueofbets_web/signout-callback-oidc"},
                ClientSecrets =
                {
                    new Secret("developer".Sha256())
                },
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile
                }
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