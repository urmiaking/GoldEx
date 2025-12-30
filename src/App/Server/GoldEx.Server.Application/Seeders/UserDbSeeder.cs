using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Authorization;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Shared.Settings;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using GoldEx.Sdk.Server.Infrastructure.Common;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Seeders;

[ScopedService]
internal class UserDbSeeder(
    IAccountService accountService,
    IEnumerable<IApplicationPolicyProvider> policyProviders,
    IOptions<UserSetting> options,
    ILogger<UserDbSeeder> logger) : IDbSeeder
{
    private readonly UserSetting _userSetting = options.Value;

    public int Order => 70;

    public async Task SeedAsync(DbSeedContext context, CancellationToken cancellationToken = default)
    {
        foreach (var item in BuiltinRoles.Roles)
            await accountService.EnsureRoleAsync(item, cancellationToken);

        await PopulateAdministratorClaimsAsync(policyProviders, accountService);

        var admin = await accountService.FindUserAsync("admin");

        if (admin is null)
        {
            await accountService.CreateUserAsync(new AppUser("مدیر سامانه",
                    _userSetting.UserName,
                    _userSetting.Email,
                    _userSetting.PhoneNumber),
                _userSetting.Password,
                [
                    BuiltinRoles.Administrators
                ],
                cancellationToken);

            logger.LogInformation($"{nameof(UserDbSeeder)}: Created default administrator user.");
        }
    }

    private static async Task PopulateAdministratorClaimsAsync(IEnumerable<IApplicationPolicyProvider> policyProviders, IAccountService accountService)
    {
        var policies = policyProviders
            .SelectMany(x => x.GetPolicies())
            .SelectMany(x => x.Policies);

        foreach (var policy in policies)
        {
            var claimRequirements = policy.Requirements.OfType<ClaimsAuthorizationRequirement>();

            foreach (var claimRequirement in claimRequirements)
                if (claimRequirement.AllowedValues is null)
                    await accountService.AddRoleClaimAsync(BuiltinRoles.Administrators,
                        new Claim(claimRequirement.ClaimType, string.Empty));
                else
                    foreach (var requiredValue in claimRequirement.AllowedValues)
                        await accountService.AddRoleClaimAsync(BuiltinRoles.Administrators, new Claim(claimRequirement.ClaimType, requiredValue));
        }
    }
}