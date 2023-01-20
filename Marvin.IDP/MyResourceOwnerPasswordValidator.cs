using System.Security.Claims;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;
using IdentityModel;

namespace Marvin.IDP
{
    public class MyResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly ILogger<MyResourceOwnerPasswordValidator> logger;

        public MyResourceOwnerPasswordValidator(ILogger<MyResourceOwnerPasswordValidator> logger)
        {
            this.logger = logger;
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            this.logger.LogInformation($"UserName: {context.UserName}, Password: {context.Password}");

            var claims = new List<Claim>
            {
                new Claim("role", "PayingUser"),
                new Claim("imagegalleryapi", "imagegalleryapi.read"),
                new Claim("imagegalleryapi", "imagegalleryapi.write"),
                new Claim(JwtClaimTypes.GivenName, "Emma"),
                new Claim(JwtClaimTypes.FamilyName, "Flagg"),
                new Claim("country", "be")
            };

            context.Result = new GrantValidationResult(context.UserName, GrantType.ResourceOwnerPassword, claims);

            return Task.CompletedTask;
        }
    }
}
