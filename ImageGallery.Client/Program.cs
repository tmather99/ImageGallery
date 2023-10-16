using System.IdentityModel.Tokens.Jwt;
using ImageGallery.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Logging;
using Microsoft.Net.Http.Headers;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    IdentityModelEventSource.ShowPII = true;

    builder.Services.AddHttpLogging(options =>
    {
        options.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders;
    });

    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration));

    var idpServerUri = builder.Configuration["IdpServerUri"];
    var imageGalleryAPIRoot = builder.Configuration["ImageGalleryAPIRoot"];

    Log.Information($"IdpServerUri = " + idpServerUri);
    Log.Information($"ImageGalleryAPIRoot = " + imageGalleryAPIRoot);

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    });

    // Add services to the container.
    builder.Services.AddControllersWithViews()
        .AddJsonOptions(configure => configure.JsonSerializerOptions.PropertyNamingPolicy = null);

    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

    builder.Services.AddAccessTokenManagement();

    // Add services to the container.
    builder.Services.AddControllers().AddDapr();
    builder.Services.AddDaprClient();

    // create an HttpClient used for accessing the API
    builder.Services.AddHttpClient("APIClient", client =>
    {
        client.BaseAddress = new Uri(imageGalleryAPIRoot);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
    }).AddUserAccessTokenHandler();

    builder.Services.AddHttpClient("IDPClient", client =>
    {
        client.BaseAddress = new Uri(idpServerUri);
    });

    builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.AccessDeniedPath = "/Authentication/AccessDenied";
        })
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        { 
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Authority = idpServerUri;
            options.ClientId = "imagegalleryclient";
            options.ClientSecret = "secret";
            options.ResponseType = "code";
            //options.Scope.Add("openid");
            //options.Scope.Add("profile");
            //options.CallbackPath = new PathString("signin-oidc");
            // SignedOutCallbackPath: default = host:port/signout-callback-oidc.
            // Must match with the post logout redirect URI at IDP client config if
            // you want to automatically return to the application after logging out
            // of IdentityServer.
            // To change, set SignedOutCallbackPath
            // eg: options.SignedOutCallbackPath = new PathString("pathaftersignout");
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.ClaimActions.Remove("aud");
            options.ClaimActions.DeleteClaim("sid");
            options.ClaimActions.DeleteClaim("idp");
            options.Scope.Add("roles");
            //options.Scope.Add("imagegalleryapi.fullaccess");
            options.Scope.Add("imagegalleryapi.read");
            options.Scope.Add("imagegalleryapi.write");
            options.Scope.Add("country");
            options.Scope.Add("offline_access");
            options.ClaimActions.MapJsonKey("role", "role");
            options.ClaimActions.MapUniqueJsonKey("country", "country");
            options.TokenValidationParameters = new()
            {
                NameClaimType = "given_name",
                RoleClaimType = "role",
            };
        });

    builder.Services.AddAuthorization(authorizationOptions =>
    {
        authorizationOptions.AddPolicy("UserCanAddImage", AuthorizationPolicies.CanAddImage());
    });

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.Use(async (context, next) =>
    {
        context.Request.Scheme = "https";

        // Connection: RemoteIp
        Log.Information("Request RemoteIp: {RemoteIpAddress}", context.Connection.RemoteIpAddress);

        await next(context);
    });

    app.UseForwardedHeaders();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHsts();
    app.UseHttpsRedirection();

    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Gallery}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
