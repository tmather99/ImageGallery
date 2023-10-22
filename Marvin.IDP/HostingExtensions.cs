using System.Reflection;
using Marvin.IDP.DbContexts;
using Marvin.IDP.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;

namespace Marvin.IDP;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        // uncomment if you want to add a UI
        builder.Services.AddRazorPages();

        builder.Services.AddScoped<IPasswordHasher<Entities.User>, PasswordHasher<Entities.User>>();

        builder.Services.AddScoped<ILocalUserService, LocalUserService>();

        //var idpDBConnectionString = builder.Configuration["ConnectionStrings:MarvinIdentityDBConnectionString"];
        var connectionString = builder.Configuration["ConnectionStrings:GlobomanticsDb"];
        Log.Information($"connectionString = " + connectionString);

        builder.Services.AddDbContext<IdentityDbContext>(options =>
        {
            //options.UseSqlite(builder.Configuration.GetConnectionString("MarvinIdentityDBConnectionString"));
            options.UseSqlServer(connectionString);
        });

        var migrationAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;

        builder.Services.AddIdentityServer(options =>
        {
            // https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
            options.EmitStaticAudienceClaim = true;
        })
        .AddProfileService<LocalUserProfileService>()
        .AddInMemoryIdentityResources(Config.IdentityResources)
        .AddInMemoryApiScopes(Config.ApiScopes)
        .AddInMemoryApiResources(Config.ApiResources)
        .AddResourceOwnerValidator<MyResourceOwnerPasswordValidator>()
        .AddMyInMemoryClients(builder.ConfigClients())
        .AddOperationalStore(options =>
        {
            options.ConfigureDbContext = 
                optionsBuilder => optionsBuilder.UseSqlServer(connectionString,
                sqlOptions => sqlOptions.MigrationsAssembly(migrationAssembly));

            options.EnableTokenCleanup = true;
        });

        var redisConnectionString = builder.Configuration["ConnectionStrings:DistributedCache"];
        var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);

        builder.Services.AddStackExchangeRedisCache(option =>
        {
            option.Configuration = redisConnectionString;
            option.InstanceName = "Redis";
        });

        builder.Services.AddDataProtection()
            .PersistKeysToStackExchangeRedis(connectionMultiplexer, "DataProtection-Keys")
            .SetApplicationName("Marvin.IDP");

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        return builder.Build();
    }
    
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        app.UseForwardedHeaders();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // uncomment if you want to add a UI
        app.UseStaticFiles();
        app.UseRouting();
            
        app.UseIdentityServer();

        // uncomment if you want to add a UI
        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}
