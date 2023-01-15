using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using IdentityModel.Client;

var deviceUdid = args.Length == 0 ? Guid.NewGuid().ToString() : args[0];
var deviceClient = new HttpClient();

var resp = await deviceClient.PostAsync(
    "https://api.imagegallery.com:7075/api/device/enroll",
    new FormUrlEncodedContent(
        new Dictionary<string, string>()
        {
            { "deviceUdid", deviceUdid }
        }));

resp.EnsureSuccessStatusCode();

var sharedSecret = await resp.Content.ReadAsStringAsync();

Console.WriteLine($"  deviceUdid: {deviceUdid}\n" +
                  $" sharedSeret: {sharedSecret}\n");

// discover endpoints from metadata
var disco = await deviceClient.GetDiscoveryDocumentAsync("https://idp.imagegallery.com:5001");
if (disco.IsError)
{
    Console.WriteLine(disco.Error);
    return;
}

// request token
var tokenResponse = await deviceClient.RequestClientCredentialsTokenAsync(
    new ClientCredentialsTokenRequest
    {
        Address = disco.TokenEndpoint,
        ClientId = deviceUdid,
        ClientSecret = sharedSecret,
    });

if (tokenResponse.IsError)
{
    Console.WriteLine(tokenResponse.Error);
    return;
}

Console.WriteLine($"Access Token: {tokenResponse.AccessToken}\n");

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
