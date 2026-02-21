using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace GoldEx.Calculator.Server.Services;

public class ConfigureSecurityStampOptions
    : IConfigureOptions<SecurityStampValidatorOptions>
{
    public void Configure(SecurityStampValidatorOptions options)
    {
        options.ValidationInterval = TimeSpan.FromMinutes(10);

        options.OnRefreshingPrincipal = refreshingPrincipal =>
        {
            var newIdentity = refreshingPrincipal.NewPrincipal?.Identities.First();
            var currentIdentity = refreshingPrincipal.CurrentPrincipal?.Identities.First();

            if (currentIdentity is not null && newIdentity is not null)
            {
                var currentClaimsNotInNewIdentity = currentIdentity.Claims.Where(c => !newIdentity.HasClaim(c.Type, c.Value));

                foreach (var claim in currentClaimsNotInNewIdentity)
                {
                    newIdentity.AddClaim(claim);
                }
            }

            return Task.CompletedTask;
        };
    }
}