using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Application.BackgroundServices;
using GoldEx.Server.Application.Factories;
using GoldEx.Server.Application.Services;
using GoldEx.Server.Application.Validators;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.SettingsAggregate;
using GoldEx.Shared.Application.Services;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Application.Validators.Prices;
using GoldEx.Shared.Application.Validators.Products;
using GoldEx.Shared.Application.Validators.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Server.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddHostedService<PriceUpdaterBackgroundService>();

        services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppUserClaimsPrincipalFactory>();

        services.AddScoped<IProductService<Product>, ServerProductService>();
        services.AddScoped<IPriceService<Price, PriceHistory>, PriceService<Price, PriceHistory>>();
        services.AddScoped<ISettingsService<Settings>, SettingsService<Settings>>();

        services.AddScoped<CreateServerProductValidator>();
        services.AddScoped<UpdateServerProductValidator>();
        services.AddScoped<DeleteProductValidator<Product>>();
        services.AddScoped<PriceValidator<Price, PriceHistory>>();
        services.AddScoped<SettingsValidator<Settings>>();

        services.DiscoverServices();
        return services;
    }
}