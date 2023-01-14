// discover endpoints from metadata
using System.Collections.Specialized;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var deviceClient = new HttpClient();
var disco = await deviceClient.GetDiscoveryDocumentAsync("https://idp.imagegallery.com:5001");
if (disco.IsError)
{
    Console.WriteLine(disco.Error);
    return;
}

// request token
var tokenResponse = await deviceClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
{
    Address = disco.TokenEndpoint,

    ClientId = "device-1",
    ClientSecret = "secret",
    Scope = "imagegalleryapi.read imagegalleryapi.write",
});


if (tokenResponse.IsError)
{
    Console.WriteLine(tokenResponse.Error);
    return;
}

Console.WriteLine("Access Token: " + tokenResponse.AccessToken);

var jwt = new JwtSecurityToken(tokenResponse.AccessToken);

foreach (var claim in jwt.Claims)
{
    Console.WriteLine($"\t{claim.Type}: {claim.Value}");
}

// call api
var apiClient = new HttpClient();
apiClient.SetBearerToken(tokenResponse.AccessToken);

var response = await apiClient.GetAsync("https://api.imagegallery.com:7075/api/images");
if (!response.IsSuccessStatusCode)
{
    Console.WriteLine(response.StatusCode);
}
else
{
    var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
    Console.WriteLine(JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }));
}
