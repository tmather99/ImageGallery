using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dapr.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Duende.IdentityServer.Models;

namespace ImageGallery.Client.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly DaprClient daprClient;
        private readonly ILogger<AuthenticationController> logger;

        private readonly IHttpClientFactory httpClientFactory;

        public AuthenticationController(
            IHttpClientFactory httpClientFactory,
            DaprClient daprClient,
            ILogger<AuthenticationController> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.daprClient = daprClient;
            this.logger = logger;
        }

        [AllowAnonymous]
        public async Task Login(string adminId, string deviceUdid)
        {
            var idpClient = this.httpClientFactory.CreateClient("IDPClient");

            var disco = await idpClient.GetDiscoveryDocumentAsync();
            if (disco.IsError)
            {
                throw new Exception(disco.Error);
            }

            var deviceState = await this.daprClient.GetStateAsync<Duende.IdentityServer.Models.Client>(storeName: "statestore", deviceUdid);

            this.logger.LogInformation($"{this.JsonSerialize(deviceState)}");


            // request token
            var tokenResponse = await idpClient.RequestPasswordTokenAsync(
                new PasswordTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = "uem",
                    ClientSecret = "secret",
                    Scope = "roles",
                    UserName = "Emma",
                    Password = "password"
                });

            if (tokenResponse.IsError)
            {
                this.logger.LogError(tokenResponse.Error);
                return;
            }

            this.logger.LogInformation($"Access Token: {tokenResponse.AccessToken}\n");

            var jwt = new JwtSecurityToken(tokenResponse.AccessToken);

            foreach (var claim in jwt.Claims)
            {
                this.logger.LogInformation($"\t{claim.Type}: {claim.Value}");
            }

            // call api
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(tokenResponse.AccessToken);

            var response = await apiClient.GetAsync("https://api.assistdevtest.com/api/images");
            if (!response.IsSuccessStatusCode)
            {
                this.logger.LogError(response.StatusCode.ToString());
            }
            else
            {
                var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
                this.logger.LogInformation(JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }));
            }
        }

        [Authorize]
        public async Task Logout()
        {
            var client = this.httpClientFactory.CreateClient("IDPClient");

            var discoveryDocumentResponse = await client.GetDiscoveryDocumentAsync();
            if (discoveryDocumentResponse.IsError)
            {
                throw new Exception(discoveryDocumentResponse.Error);
            }

            var accessTokenRevocationResponse = await client.RevokeTokenAsync(
                new()
                {
                    Address = discoveryDocumentResponse.RevocationEndpoint,
                    ClientId = "imagegalleryclient",
                    ClientSecret = "secret",
                    Token = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken)
                });

            if (accessTokenRevocationResponse.IsError)
            {
                throw new Exception(accessTokenRevocationResponse.Error);
            }

            var refreshTokenRevocationResponse = await client.RevokeTokenAsync(
                new()
                {
                    Address = discoveryDocumentResponse.RevocationEndpoint,
                    ClientId = "imagegalleryclient",
                    ClientSecret = "secret",
                    Token = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken)
                });

            if (refreshTokenRevocationResponse.IsError)
            {
                throw new Exception(accessTokenRevocationResponse.Error);
            }

            // Clears the local cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirects to the IDP linked to scheme
            // "OpenIdConnectDefaults.AuthenticationScheme" (oidc)
            // so it can clear its own session/cookie
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }

        public IActionResult AccessDenied()
        {
            return View();
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