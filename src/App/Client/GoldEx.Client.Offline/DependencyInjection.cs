using GoldEx.Client.Offline.Domain.CustomerAggregate;
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
using GoldEx.Shared.Application.Validators.Customers;
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

        services.AddScoped<IProductRepository<Product, ProductCategory, GemStone>, ProductRepository<Product, ProductCategory, GemStone>>();
        services.AddScoped<IProductService<Product, ProductCategory, GemStone>, ProductService<Product, ProductCategory, GemStone>>();

        services.AddScoped<IPriceRepository<Price, PriceHistory>, PriceRepository<Price, PriceHistory>>();
        services.AddScoped<IPriceService<Price, PriceHistory>, PriceService<Price, PriceHistory>>();

        services.AddScoped<ISettingsRepository<Settings>, SettingsRepository<Settings>>();
        services.AddScoped<ISettingsService<Settings>, SettingsService<Settings>>();

        services.AddScoped<IProductCategoryRepository<ProductCategory>, ProductCategoryRepository<ProductCategory>>();
        services.AddScoped<IProductCategoryService<ProductCategory>, ProductCategoryService<ProductCategory, Product, GemStone>>();

        services.AddScoped<ICustomerRepository<Customer>, CustomerRepository<Customer>>();
        services.AddScoped<ICustomerService<Customer>, CustomerService<Customer>>();

        services.AddScoped<CreateProductValidator<Product, ProductCategory, GemStone>>();
        services.AddScoped<UpdateProductValidator<Product, ProductCategory, GemStone>>();
        services.AddScoped<DeleteProductValidator<Product, ProductCategory, GemStone>>();
        services.AddScoped<PriceValidator<Price, PriceHistory>>();
        services.AddScoped<SettingsValidator<Settings>>();
        services.AddScoped<CreateProductCategoryValidator<ProductCategory>>();
        services.AddScoped<UpdateProductCategoryValidator<ProductCategory>>();
        services.AddScoped<DeleteProductCategoryValidator<ProductCategory, Product, GemStone>>();
        services.AddScoped<CreateCustomerValidator<Customer>>();
        services.AddScoped<UpdateCustomerValidator<Customer>>();
        services.AddScoped<DeleteCustomerValidator<Customer>>();

        services.DiscoverServices();
        return services;
    }
}