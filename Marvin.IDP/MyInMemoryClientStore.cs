using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;

namespace Duende.IdentityServer.Stores;

/// <summary>
/// In-memory client store
/// </summary>
public class MyInMemoryClientStore : IClientStore
{
    private readonly IEnumerable<Client> _clients;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryClientStore"/> class.
    /// </summary>
    /// <param name="clients">The clients.</param>
    public MyInMemoryClientStore(IEnumerable<Client> clients)
    {
        if (clients.HasDuplicates(m => m.ClientId))
        {
            throw new ArgumentException("Clients must not contain duplicate ids");
        }
        _clients = clients;
    }

    /// <summary>
    /// Finds a client by id
    /// </summary>
    /// <param name="clientId">The client id</param>
    /// <returns>
    /// The client
    /// </returns>
    public Task<Client> FindClientByIdAsync(string clientId)
    {
        var query = from client in _clients
                    where client.ClientId == clientId
                    select client;

        if (query.Any())
        {
            return Task.FromResult(query.SingleOrDefault());
        }

        var device = new Client
        {
            ClientId = "client",
            ClientName = "Client Crendential",
            ClientSecrets =
            {
                new Secret("secret".Sha256())
            },
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            AllowedScopes =
            {
                "imagegalleryapi.read",
                "imagegalleryapi.write"
            }
        };

        return Task.FromResult(device);
    }
}