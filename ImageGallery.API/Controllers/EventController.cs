using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Dapr.Client;
using Duende.IdentityServer.Models;
using Dapr;

namespace ImageGallery.API.Controllers
{
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly DaprClient daprClient;
        private readonly ILogger<EventController> logger;

        public EventController(DaprClient daprClient, ILogger<EventController> logger)
        {
            this.daprClient = daprClient;
            this.logger = logger;
        }

        [Topic("pubsub", "state")]
        [HttpPost("state")]
        public async Task<Client> State(Client deviceState)
        {
            this.logger.LogInformation($"{this.JsonSerialize(deviceState)}");

            return deviceState;
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