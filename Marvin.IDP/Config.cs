using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
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

    public static IEnumerable<Client> ConfigClients(this WebApplicationBuilder builder)
    {
        var redirectUris = builder.Configuration?.GetSection("Client")["RedirectUris"];
        var postLogoutRedirectUris = builder.Configuration?.GetSection("Client")["PostLogoutRedirectUris"];

        Log.Information($"RedirectUris = " + redirectUris);
        Log.Information($"PostLogoutRedirectUris = " + postLogoutRedirectUris);

        return new List<Client>
        {
            new Client
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
                    redirectUris
                    //"https://client.imagegallery.com:7184/signin-oidc"
                },
                PostLogoutRedirectUris =
                {
                    postLogoutRedirectUris
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
    }

    /// <summary>
    /// Adds the in memory clients.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="clients">The clients.</param>
    /// <returns></returns>
    public static IIdentityServerBuilder AddMyInMemoryClients(this IIdentityServerBuilder builder, IEnumerable<Client> clients)
    {
        builder.Services.AddSingleton(clients);
        builder.AddClientStore<MyInMemoryClientStore>();
        return builder;
    }

}