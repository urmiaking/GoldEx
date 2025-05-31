using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Authorization;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Infrastructure;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Services;
using GoldEx.Shared.Settings;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace GoldEx.Server.Extensions;

public static class ServiceProviderExtensions
{
    internal static async Task MigrateAsync(this IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<GoldExDbContext>();
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

        if (pendingMigrations.Any())
            await dbContext.Database.MigrateAsync();
    }

    internal static async Task SeedAsync(this IServiceProvider serviceProvider)
    {
        await EnsureDatabasePopulated(serviceProvider);
    }

    private static async Task EnsureDatabasePopulated(IServiceProvider serviceProvider)
    {
        await PopulateDefaultSettingsAsync(serviceProvider);

        var accountService = serviceProvider.GetRequiredService<IAccountService>();
        var policyProviders = serviceProvider.GetServices<IApplicationPolicyProvider>();

        foreach (var item in BuiltinRoles.Roles)
            await accountService.EnsureRoleAsync(item);

        await PopulateAdministratorClaimsAsync(policyProviders, accountService);

        var admin = await accountService.FindUserAsync("admin");

        if (admin is null)
        {
            var adminUser = serviceProvider.GetRequiredService<IOptions<UserSetting>>().Value;

            await accountService.CreateUserAsync(new AppUser("مدیر سامانه", adminUser.UserName, adminUser.Email, adminUser.PhoneNumber),
                adminUser.Password, [BuiltinRoles.Administrators]);
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

    private static async Task PopulateDefaultSettingsAsync(IServiceProvider provider)
    {
        var serverSettingService = provider.GetRequiredService<IServerSettingService>();
        var settingsService = provider.GetRequiredService<ISettingService>();

        var settings = await settingsService.GetAsync();

        if (settings is null)
        {
            var defaultSettingOptions = provider.GetRequiredService<IOptions<DefaultSetting>>();
            var defaultSetting = defaultSettingOptions.Value;

            var setting = new CreateSettingRequest(defaultSetting.InstitutionName,
                defaultSetting.Address,
                defaultSetting.PhoneNumber,
                defaultSetting.TaxPercent,
                defaultSetting.GoldProfitPercent,
                defaultSetting.JewelryProfitPercent);

            await serverSettingService.CreateAsync(setting);
        }
    }
}