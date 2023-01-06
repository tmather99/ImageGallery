using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Serilog;

namespace Marvin.IDP;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        { 
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource("roles", "Your role(s)", new [] { "role" }),
            new IdentityResource("country", "The country you're living in", new List<string>() { "country" })
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource("imagegalleryapi", "Image Gallery API", new [] { "role", "country" })
            {
                Scopes = { "imagegalleryapi.fullaccess",
                           "imagegalleryapi.read",
                           "imagegalleryapi.write"},
                ApiSecrets = { new Secret("apisecret".Sha256()) }
            }
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("imagegalleryapi.fullaccess"),
            new ApiScope("imagegalleryapi.read"),
            new ApiScope("imagegalleryapi.write")
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            new Client()
            {
                ClientName = "Image Gallery",
                ClientId = "imagegalleryclient",
                AllowedGrantTypes = GrantTypes.Code,
                AccessTokenType = AccessTokenType.Reference,
                // AuthorizationCodeLifetime = ...
                // IdentityTokenLifetime = ...
                AllowOfflineAccess = true,
                UpdateAccessTokenClaimsOnRefresh = true,
                AccessTokenLifetime = 120,
                RedirectUris =
                {
                    //"https://client.imagegallery.com:7184/signin-oidc"
                },
                PostLogoutRedirectUris =
                {
                    //"https://client.imagegallery.com:7184/signout-callback-oidc"
                },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "roles",
                    //"imagegalleryapi.fullaccess",
                    "imagegalleryapi.read",
                    "imagegalleryapi.write",
                    "country"
                },
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                }, 
                //RequireConsent = true
            }
        };

    public static IEnumerable<Client> ConfigClients(this WebApplicationBuilder builder)
    {
        var redirectUris = builder.Configuration?.GetSection("Client")["RedirectUris"];
        var postLogoutRedirectUris = builder.Configuration?.GetSection("Client")["PostLogoutRedirectUris"];

        Log.Information($"RedirectUris = " + redirectUris);
        Log.Information($"PostLogoutRedirectUris = " + postLogoutRedirectUris);

        var client = Clients.First();
        client.RedirectUris = new List<string>
        {
            redirectUris
        };

        client.PostLogoutRedirectUris = new List<string>
        {
            postLogoutRedirectUris
        };

        return new List<Client>
        {
            client
        };
    }
}