using Microsoft.Extensions.DependencyInjection;
using GoldEx.Client.Offline.Domain.PriceAggregate;
using GoldEx.Client.Offline.Domain.ProductAggregate;
using GoldEx.Client.Offline.Domain.ProductCategoryAggregate;
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
using GoldEx.Shared.Application.Validators.Categories;
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

        services.AddScoped<IProductRepository<Product, ProductCategory>, ProductRepository<Product, ProductCategory>>();
        services.AddScoped<IProductService<Product, ProductCategory>, ProductService<Product, ProductCategory>>();

        services.AddScoped<IPriceRepository<Price, PriceHistory>, PriceRepository<Price, PriceHistory>>();
        services.AddScoped<IPriceService<Price, PriceHistory>, PriceService<Price, PriceHistory>>();

        services.AddScoped<ISettingsRepository<Settings>, SettingsRepository<Settings>>();
        services.AddScoped<ISettingsService<Settings>, SettingsService<Settings>>();

        services.AddScoped<IProductCategoryRepository<ProductCategory>, ProductCategoryRepository<ProductCategory>>();
        services.AddScoped<IProductCategoryService<ProductCategory>, ProductCategoryService<ProductCategory, Product>>();

        services.AddScoped<CreateProductValidator<Product, ProductCategory>>();
        services.AddScoped<UpdateProductValidator<Product, ProductCategory>>();
        services.AddScoped<DeleteProductValidator<Product, ProductCategory>>();
        services.AddScoped<PriceValidator<Price, PriceHistory>>();
        services.AddScoped<SettingsValidator<Settings>>();
        services.AddScoped<CreateProductCategoryValidator<ProductCategory>>();
        services.AddScoped<UpdateProductCategoryValidator<ProductCategory>>();
        services.AddScoped<DeleteProductCategoryValidator<ProductCategory, Product>>();

        services.DiscoverServices();
        return services;
    }
}