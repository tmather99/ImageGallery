﻿using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using IdentityModel.Client;

var deviceUdid = args.Length == 0 ? Guid.NewGuid().ToString() : args[0];
var deviceClient = new HttpClient();

var resp = await deviceClient.PostAsync(
    "https://api.assistdevtest.com/api/device/enroll",
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
var disco = await deviceClient.GetDiscoveryDocumentAsync("https://idp.assistdevtest.com");
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

var response = await apiClient.GetAsync("https://api.assistdevtest.com/api/images");
if (!response.IsSuccessStatusCode)
{
    Console.WriteLine(response.StatusCode);
}
else
{
    var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
    Console.WriteLine(JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }));
}




// request token
tokenResponse = await deviceClient.RequestPasswordTokenAsync(
    new PasswordTokenRequest
    {
        Address = disco.TokenEndpoint,
        ClientId = "uem",
        ClientSecret = "secret",
        Scope = "openid profile roles country imagegalleryapi.read imagegalleryapi.write",
        UserName = "Emma",
        Password = "password"
    });

if (tokenResponse.IsError)
{
    Console.WriteLine(tokenResponse.Error);
    return;
}

Console.WriteLine($"Access Token: {tokenResponse.AccessToken}\n");

jwt = new JwtSecurityToken(tokenResponse.AccessToken);

foreach (var claim in jwt.Claims)
{
    Console.WriteLine($"\t{claim.Type}: {claim.Value}");
}

// call api
apiClient = new HttpClient();
apiClient.SetBearerToken(tokenResponse.AccessToken);

response = await apiClient.GetAsync("https://api.assistdevtest.com/api/images");
if (!response.IsSuccessStatusCode)
{
    Console.WriteLine(response.StatusCode);
}
else
{
    var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
    Console.WriteLine(JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }));
}


