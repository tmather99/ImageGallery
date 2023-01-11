using System.IdentityModel.Tokens.Jwt;
using ImageGallery.API.Authorization;
using ImageGallery.API.DbContexts;
using ImageGallery.API.Services;
using ImageGallery.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try 
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration));

    builder.Services.AddControllers()
        .AddJsonOptions(configure => configure.JsonSerializerOptions.PropertyNamingPolicy = null);

    var connectionString = builder.Configuration["ConnectionStrings:GlobomanticsDb"];
    Log.Information($"connectionString = " + connectionString);

    var idpServerUri = builder.Configuration["IdpServerUri"];
    Log.Information($"IdpServerUri = " + idpServerUri);

    builder.Services.AddDbContext<GalleryContext>(options =>
    {
        //options.UseSqlite(builder.Configuration["ConnectionStrings:ImageGalleryDBConnectionString"]);
        options.UseSqlServer(connectionString);
    });

    // register the repository
    builder.Services.AddScoped<IGalleryRepository, GalleryRepository>();
    builder.Services.AddScoped<IAuthorizationHandler, MustOwnImageHandler>();
    builder.Services.AddHttpContextAccessor();

    // register AutoMapper-related services
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
          // .AddJwtBearer(options =>
          // {
          //     options.Authority = builder.Configuration["IdpServerUri"];
          //     options.Audience = "imagegalleryapi";
          //     options.TokenValidationParameters = new ()
          //     {
          //         NameClaimType = "given_name",
          //         RoleClaimType = "role",
          //         ValidTypes = new[] { "at+jwt" }
          //     };
          // });
          .AddOAuth2Introspection(options =>
          {
              options.Authority = idpServerUri;
              options.ClientId = "imagegalleryapi";
              options.ClientSecret = "apisecret";
              options.NameClaimType = "given_name";
              options.RoleClaimType = "role";
          });

    builder.Services.AddAuthorization(authorizationOptions =>
    {
        authorizationOptions.AddPolicy(
            "UserCanAddImage", AuthorizationPolicies.CanAddImage());
        authorizationOptions.AddPolicy(
            "ClientApplicationCanWrite", policyBuilder =>
            {
                policyBuilder.RequireClaim("scope", "imagegalleryapi.write");
            });
        authorizationOptions.AddPolicy(
            "MustOwnImage", policyBuilder =>
            {
                policyBuilder.RequireAuthenticatedUser();
                policyBuilder.AddRequirements(new MustOwnImageRequirement());

            });
    });

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();

    // Configure the HTTP request pipeline.
    app.UseHsts();
    app.UseHttpsRedirection();

    app.UseStaticFiles();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex.GetType().Name is not "HostAbortedException")
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
