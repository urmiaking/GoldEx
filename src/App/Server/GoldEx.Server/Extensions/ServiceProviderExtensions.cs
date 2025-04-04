﻿using GoldEx.Sdk.Common.Authorization;
using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Infrastructure;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Security.Claims;
using GoldEx.Server.Domain.SettingsAggregate;
using GoldEx.Shared.Application.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

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
            await accountService.CreateUserAsync(new AppUser("مدیر سامانه", "admin@admin.com", "admin@admin.com", "09905492104"),
                            "admin", [BuiltinRoles.Administrators]);
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
            {
                if (claimRequirement.AllowedValues is null)
                {
                    await accountService.AddRoleClaimAsync(BuiltinRoles.Administrators, new Claim(claimRequirement.ClaimType, string.Empty));
                }
                else
                {
                    foreach (var requiredValue in claimRequirement.AllowedValues)
                    {
                        await accountService.AddRoleClaimAsync(BuiltinRoles.Administrators, new Claim(claimRequirement.ClaimType, requiredValue));
                    }
                }
            }
        }
    }

    private static async Task PopulateDefaultSettingsAsync(IServiceProvider provider)
    {
        var settingsService = provider.GetRequiredService<ISettingsService<Settings>>();

        var settings = await settingsService.GetAsync();

        if (settings is null)
        {
            await settingsService.CreateAsync(new Settings("جواهری دمو", "ارومیه خیابان مدنی 2، پلاک 17",
                "04431934291 - 04431934119", 9, 7, 20));
        }
    }
}