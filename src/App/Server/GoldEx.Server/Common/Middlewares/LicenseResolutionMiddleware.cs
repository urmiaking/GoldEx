using GoldEx.Sdk.Server.Application.Models;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.AppLicenseAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Shared.DTOs.Licenses;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Common.Middlewares;

public class LicenseResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context, 
        IStoreContext storeContext, 
        ProductLicense productLicense, 
        ILicenseCache licenseCache,
        GoldExDbContext dbContext,
        IConfiguration configuration)
    {
        var licenseMode = configuration["License:Mode"] ?? "Hybrid";
        
        // 1. Determine target store ID
        var targetStoreId = licenseMode.Equals("InstanceWide", StringComparison.OrdinalIgnoreCase)
            ? Guid.Empty
            : (storeContext.StoreId ?? Guid.Empty);

        // 2. Fetch from cache
        var licenseInfo = licenseCache.Get(targetStoreId);

        if (licenseInfo is null)
        {
            // 3. Cache miss: read from DB ignoring query filters
            var appLicense = await dbContext.Set<AppLicense>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.StoreId == new StoreId(targetStoreId), context.RequestAborted);

            if (appLicense is not null)
            {
                licenseInfo = new GetLicenseResponse(
                    appLicense.Plan, 
                    appLicense.RegisteredAt, 
                    appLicense.ExpireDate);
            }
            else
            {
                // Unregistered fallback
                licenseInfo = new GetLicenseResponse(
                    LicensePlan.Unregistered, 
                    DateTime.MinValue, 
                    DateTime.MinValue);
            }

            // Update cache
            licenseCache.Set(targetStoreId, licenseInfo);
        }

        // 4. Update the scoped ProductLicense
        productLicense.UpdateLicense(targetStoreId, licenseInfo.Plan, licenseInfo.RegisteredAt, licenseInfo.ExpireDate);

        await next(context);
    }
}
