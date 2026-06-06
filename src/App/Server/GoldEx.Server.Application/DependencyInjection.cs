using DevExpress.XtraReports.Web.Extensions;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Sdk.Server.Application.Abstractions;
using GoldEx.Sdk.Server.Application.Services;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Application.BackgroundServices;
using GoldEx.Server.Application.Factories;
using GoldEx.Server.Application.Reporting;
using GoldEx.Server.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Server.Application;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppUserClaimsPrincipalFactory>();
            services.AddScoped<ReportStorageWebExtension, ReportStorageExtension>();
            services.AddScoped<ITransactionContext, TransactionContext<GoldExDbContext>>();

            services.DiscoverServices();
            return services;
        }

        public IServiceCollection AddHostedServices()
        {
            services.AddHostedService<PriceUpdaterBackgroundService>();
            services.AddHostedService<LicenseUpdaterBackgroundService>();
            services.AddHostedService<NotificationBackgroundService>();
            services.AddHostedService<BlogDiskSyncManager>();

            return services;
        }
    }
}