using GoldEx.Server.Application.Extensions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.AppLicenseAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Shared.DTOs.Licenses;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VHDLicenseManager;

namespace GoldEx.Server.Application.BackgroundServices;

public class LicenseUpdaterBackgroundService(
    License licenseManager,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<LicenseUpdaterBackgroundService> logger) : BackgroundService
{
    private const string ClassName = nameof(LicenseUpdaterBackgroundService);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{Name} is running.", ClassName);
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Quick startup delay

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GoldExDbContext>();
                var licenseCache = scope.ServiceProvider.GetRequiredService<ILicenseCache>();
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var licenseMode = config["License:Mode"] ?? "Hybrid";

                var allLicenses = await dbContext.Set<AppLicense>()
                    .IgnoreQueryFilters()
                    .ToListAsync(stoppingToken);

                foreach (var appLicense in allLicenses)
                {
                    var storeGuid = appLicense.StoreId.Value;
                    var isMaster = storeGuid == Guid.Empty;

                    if (isMaster)
                    {
                        try
                        {
                            var incomingLicense = await licenseManager.GetLicenseAsync(nameof(GoldEx), appLicense.LicenseId, stoppingToken);

                            if (incomingLicense is null)
                            {
                                logger.LogError("Master license is not available on the server, fallback to unregistered");
                                appLicense.UpdateSubscription(LicensePlan.Unregistered, DateTime.MinValue, DateTime.MinValue);
                            }
                            else
                            {
                                var newPlan = incomingLicense.Type.GetLicensePlan();
                                if (appLicense.Plan != newPlan || appLicense.ExpireDate.Date != incomingLicense.Expiry.Date)
                                {
                                    logger.LogInformation("Master license updated. New plan: {Plan}, Expire date: {Expiry}", newPlan, incomingLicense.Expiry);
                                    appLicense.UpdateSubscription(newPlan, incomingLicense.RegisteredAt, incomingLicense.Expiry);
                                }
                            }
                            await dbContext.SaveChangesAsync(stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Failed to sync master license from remote server");
                        }
                    }
                    else if (licenseMode.Equals("Hybrid", StringComparison.OrdinalIgnoreCase))
                    {
                        // Locally check tenant license expiration
                        if (appLicense.Plan != LicensePlan.Unregistered && appLicense.ExpireDate < DateTime.Now)
                        {
                            logger.LogInformation("Tenant store {StoreId} subscription expired. Mark as unregistered.", storeGuid);
                            appLicense.UpdateSubscription(LicensePlan.Unregistered, appLicense.RegisteredAt, appLicense.ExpireDate);
                            await dbContext.SaveChangesAsync(stoppingToken);
                        }
                    }

                    // Push/refresh in cache
                    var cacheResponse = new GetLicenseResponse(appLicense.Plan, appLicense.RegisteredAt, appLicense.ExpireDate);
                    licenseCache.Set(storeGuid, cacheResponse);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "License background update cycle failed");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}