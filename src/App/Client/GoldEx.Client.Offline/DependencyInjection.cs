using Microsoft.Extensions.DependencyInjection;
using GoldEx.Client.Offline.Domain.PriceAggregate;
using GoldEx.Client.Offline.Domain.ProductAggregate;
using GoldEx.Client.Offline.Domain.SettingsAggregate;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Shared.Application.Services;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Infrastructure.Repositories;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;
using GoldEx.Client.Offline.Infrastructure;
using GoldEx.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using GoldEx.Sdk.Common.Interceptors;
using GoldEx.Shared.Application.Validators.Products;
using GoldEx.Shared.Application.Validators.Prices;
using GoldEx.Shared.Application.Validators.Settings;

namespace GoldEx.Client.Offline;

public static class DependencyInjection
{
    public static IServiceCollection AddOfflineClient(this IServiceCollection services)
    {
        services.AddBesqlDbContextFactory<OfflineDbContext>((_, optionsBuilder) =>
        {
            optionsBuilder.UseSqlite("Data Source=Offline-ClientDb.db");
            optionsBuilder.AddInterceptors(new PersianizerInterceptor());
        });

        services.AddScoped<IGoldExDbContextFactory, OfflineDbContextFactory>();

        services.AddScoped<IProductRepository<Product>, ProductRepository<Product>>();
        services.AddScoped<IProductService<Product>, ProductService<Product>>();
        services.AddScoped<ISettingsService<Settings>, SettingsService<Settings>>();

        services.AddScoped<IPriceRepository<Price, PriceHistory>, PriceRepository<Price, PriceHistory>>();
        services.AddScoped<IPriceService<Price, PriceHistory>, PriceService<Price, PriceHistory>>();
        services.AddScoped<ISettingsRepository<Settings>, SettingsRepository<Settings>>();

        services.AddScoped<CreateProductValidator<Product>>();
        services.AddScoped<UpdateProductValidator<Product>>();
        services.AddScoped<DeleteProductValidator<Product>>();
        services.AddScoped<PriceValidator<Price, PriceHistory>>();
        services.AddScoped<SettingsValidator<Settings>>();

        services.DiscoverServices();
        return services;
    }
}