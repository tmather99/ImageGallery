using System.Diagnostics;
using Dapr.Client;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;

namespace Duende.IdentityServer.Stores;

/// <summary>
/// In-memory client store
/// </summary>
public class MyInMemoryClientStore : IClientStore
{
    private readonly DaprClient daprClient;
    private readonly IEnumerable<Client> _clients;
    private readonly ILogger<MyInMemoryClientStore> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryClientStore"/> class.
    /// </summary>
    /// <param name="clients">The clients.</param>
    public MyInMemoryClientStore(DaprClient daprClient, IEnumerable<Client> clients, ILogger<MyInMemoryClientStore> logger)
    {
        this.daprClient = daprClient;
        this.logger = logger;

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
    public async Task<Client> FindClientByIdAsync(string clientId)
    {
        //Debugger.Launch();

        var query = from client in _clients
                    where client.ClientId == clientId
                    select client;

        if (query.Any())
        {
            var client = query.SingleOrDefault();
            this.logger.LogInformation(this.JsonSerialize(client));
            return client;
        }

        var metadata = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" }
        };

        var device = new Client
        {
            ClientId = "device-1",
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
            },
            Claims = new List<ClientClaim>
            {
                new ClientClaim(type: "TenantId", value: "OG-1")
            }
        };

        var deviceState = await this.daprClient.GetStateAsync<Client>(storeName: "statestore", clientId, metadata: metadata);

        if (deviceState != null)
        {
            this.logger.LogInformation(this.JsonSerialize(deviceState));

            device.ClientId = deviceState.ClientId;
            device.ClientSecrets = deviceState.ClientSecrets;
            device.Claims = deviceState.Claims;
        }

        return device;
    }
    
    private string JsonSerialize<T>(T thing)
    {
        var jsonSerializer = new Newtonsoft.Json.JsonSerializer();
        var sw = new StringWriter();
        var writer = new Newtonsoft.Json.JsonTextWriter(sw);
        writer.Formatting = Newtonsoft.Json.Formatting.Indented;
        jsonSerializer.Serialize(writer, thing);
        return sw.ToString();
    }
}