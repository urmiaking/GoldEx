using DevExpress.XtraReports.Web.Extensions;
using DevExpress.XtraReports.Web.WebDocumentViewer.Native.Services;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Application.BackgroundServices;
using GoldEx.Server.Application.Factories;
using GoldEx.Server.Application.Reporting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Server.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddHostedService<PriceUpdaterBackgroundService>();

        services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppUserClaimsPrincipalFactory>();
        services.AddScoped<ReportStorageWebExtension, ReportStorageExtension>();

        services.DiscoverServices();
        return services;
    }
}