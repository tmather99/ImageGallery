using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Dapr.Client;
using Duende.IdentityServer.Models;

namespace ImageGallery.API.Controllers
{
    [Route("api/device")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly DaprClient daprClient;
        private readonly ILogger<DeviceController> logger;

        public DeviceController(DaprClient daprClient, ILogger<DeviceController> logger)
        {
            this.daprClient = daprClient;
            this.logger = logger;
        }

        [HttpPost("enroll", Name = "Enroll")]
        public async Task<ActionResult<string>> Enroll([FromForm] Dictionary<string, string> formContent)
        {
            //Debugger.Launch();

            var deviceUdid = formContent["deviceUdid"];
            var deviceState = await this.daprClient.GetStateAsync<Client>(storeName: "statestore", deviceUdid);

            if (deviceState != null)
            {
                return Ok(await this.SaveDeviceStateAsync(deviceState));
            }

            deviceState = new Client
            {
                ClientId = deviceUdid,
                ClientName = "Client Crendential",
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

            return Ok(await this.SaveDeviceStateAsync(deviceState));
        }

        private async Task<string> SaveDeviceStateAsync(Client deviceState)
        {
            var deviceUdid = deviceState.ClientId;
            var sharedSecret = Guid.NewGuid().ToString();

            deviceState.ClientSecrets = new List<Secret>
            {
                new Secret(sharedSecret.Sha256())
            };

            var metadata = new Dictionary<string, string>
            {
                { "ttlInSeconds", Guid.TryParse(deviceUdid, out _) ? "15" : "-1" }
            };

            this.logger.LogInformation($"ttl: {metadata["ttlInSeconds"]}\nsharedSecrets: {sharedSecret}\n{this.JsonSerialize(deviceState)}");

            await this.daprClient.SaveStateAsync<Client>(storeName: "statestore", deviceUdid, value: deviceState, metadata: metadata);

            return sharedSecret;
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
}