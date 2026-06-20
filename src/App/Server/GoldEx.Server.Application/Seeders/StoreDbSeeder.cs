using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Sdk.Server.Infrastructure.Common;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Server.Application.Services.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GoldEx.Server.Application.Seeders;

[ScopedService]
internal class StoreDbSeeder(
    GoldExDbContext dbContext,
    IAccountService accountService,
    IWebHostEnvironment webHostEnvironment,
    ILogger<StoreDbSeeder> logger) : IDbSeeder
{
    public int Order => 80;

    public async Task SeedAsync(DbSeedContext context, CancellationToken cancellationToken = default)
    {
        // Copy Logo & Reports for Default Store if they do not exist
        try
        {
            var appIconDir = Path.Combine(webHostEnvironment.ContentRootPath, "uploads", "icons", "app");
            var defaultIconPath = Path.Combine(appIconDir, "logo.png");
            var storeIconPath = Path.Combine(appIconDir, "logo_default.png");
            if (File.Exists(defaultIconPath) && !File.Exists(storeIconPath))
            {
                if (!Directory.Exists(appIconDir))
                {
                    Directory.CreateDirectory(appIconDir);
                }
                File.Copy(defaultIconPath, storeIconPath);
                logger.LogInformation("Copied default app icon logo.png to logo_default.png.");
            }

            var reportsDir = Path.Combine(webHostEnvironment.ContentRootPath, "Reports");
            var defaultReportPath = Path.Combine(reportsDir, "InvoiceReport.repx");
            var storeReportPath = Path.Combine(reportsDir, "InvoiceReport_default.repx");
            if (File.Exists(defaultReportPath) && !File.Exists(storeReportPath))
            {
                if (!Directory.Exists(reportsDir))
                {
                    Directory.CreateDirectory(reportsDir);
                }
                File.Copy(defaultReportPath, storeReportPath);
                logger.LogInformation("Copied default InvoiceReport.repx to InvoiceReport_default.repx.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to copy default templates/icons.");
        }
        var defaultStoreId = new StoreId(Guid.Empty);

        // 1. Ensure Default Store exists
        var defaultStore = await dbContext.Set<Store>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == defaultStoreId, cancellationToken);

        if (defaultStore == null)
        {
            defaultStore = Store.CreateDefaultStore("فروشگاه مرکزی", "default");
            await dbContext.Set<Store>().AddAsync(defaultStore, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded default store (Guid.Empty).");
        }

        // 2. Fetch Administrators and Owners from Account Service
        var adminUsers = await accountService.GetRoleMembersAsync(BuiltinRoles.Administrators, cancellationToken);
        var ownerUsers = await accountService.GetRoleMembersAsync(BuiltinRoles.Owners, cancellationToken);

        var targetUsers = adminUsers.Concat(ownerUsers)
            .GroupBy(u => u.Id)
            .Select(g => g.First())
            .ToList();

        if (targetUsers.Count > 0)
        {
            var userIds = targetUsers.Select(u => u.Id).ToList();

            // 3. Find existing store assignments for these users
            var existingMappings = await dbContext.Set<StoreUser>()
                .Where(su => userIds.Contains(su.UserId))
                .ToListAsync(cancellationToken);

            var mappedUserIds = existingMappings.Select(su => su.UserId).ToHashSet();

            // 4. Create default mapping only for users who have no assignments at all
            var newMappings = targetUsers
                .Where(u => !mappedUserIds.Contains(u.Id))
                .Select(u => StoreUser.Create(defaultStoreId, u.Id, isDefault: true))
                .ToList();

            if (newMappings.Count > 0)
            {
                await dbContext.Set<StoreUser>().AddRangeAsync(newMappings, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Assigned {Count} existing Admin/Owner users to the default store.", newMappings.Count);
            }
        }
    }
}
