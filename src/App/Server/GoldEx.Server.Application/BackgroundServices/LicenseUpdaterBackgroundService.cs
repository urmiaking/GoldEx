using GoldEx.Sdk.Server.Application.Models;
using GoldEx.Server.Application.Extensions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VHDLicenseManager;

namespace GoldEx.Server.Application.BackgroundServices;

public class LicenseUpdaterBackgroundService(
    License licenseManager,
    ProductLicense license,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<LicenseUpdaterBackgroundService> logger) : BackgroundService
{
    private const string ClassName = nameof(LicenseUpdaterBackgroundService);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{Name} is running.", ClassName);
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();

                var licenseStore = scope.ServiceProvider.GetRequiredService<ILicenseStore>();

                var appLicense = await licenseStore.GetAsync(stoppingToken);

                if (appLicense is null)
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                    continue;
                }

                var incomingLicense = await licenseManager.GetLicenseAsync(nameof(GoldEx), appLicense.LicenseId, stoppingToken);

                if (incomingLicense is null)
                {
                    logger.LogError("License is not available on the server, fallback to unregistered plan");
                    license.UpdateLicense(LicensePlan.Unregistered, DateTime.MinValue, DateTime.MinValue);
                    await licenseStore.DeleteAsync(stoppingToken);
                    continue;
                }

                if (incomingLicense.Type.GetLicensePlan() != license.Plan ||
                    incomingLicense.Expiry.ToUniversalTime().Date != license.ExpireDate.ToUniversalTime().Date)
                {
                    logger.LogInformation($"License updated. new plan {incomingLicense.Type.GetLicensePlan().ToString()}, Expire date: {incomingLicense.Expiry}");
                    license.UpdateLicense(incomingLicense.Type.GetLicensePlan(), incomingLicense.RegisteredAt, incomingLicense.Expiry);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "License refresh failed");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}