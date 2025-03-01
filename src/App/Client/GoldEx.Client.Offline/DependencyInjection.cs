using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using GoldEx.Client.Offline.Domain.PriceAggregate;
using GoldEx.Client.Offline.Domain.PriceHistoryAggregate;
using GoldEx.Client.Offline.Domain.ProductAggregate;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Shared.Application.Services;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Application.Validators;
using GoldEx.Shared.Infrastructure.Repositories;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;
using GoldEx.Client.Offline.Infrastructure;
using GoldEx.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace GoldEx.Client.Offline;

public static class DependencyInjection
{
    public static IServiceCollection AddOfflineClient(this IServiceCollection services)
    {
        services.AddMapsterConfig();

        services.AddBesqlDbContextFactory<OfflineDbContext>((_, optionsBuilder) =>
        {
            optionsBuilder
                .UseSqlite("Data Source=Offline-ClientDb.db");
        }, async (_, dbContext) => await dbContext.Database.MigrateAsync());

        services.AddScoped<IGoldExDbContextFactory, OfflineDbContextFactory>();

        services.AddScoped<IProductRepository<Product>, ProductRepository<Product>>();
        services.AddScoped<IProductService<Product>, ProductService<Product>>();

        services.AddScoped<IPriceRepository<Price, PriceHistory>, PriceRepository<Price, PriceHistory>>();
        services.AddScoped<IPriceService<Price, PriceHistory>, PriceService<Price, PriceHistory>>();

        services.AddScoped<IPriceHistoryRepository<PriceHistory>, PriceHistoryRepository<PriceHistory>>();
        services.AddScoped<IPriceHistoryService<PriceHistory>, PriceHistoryService<PriceHistory>>(); 

        services.AddScoped<CreateProductValidator<Product>>();
        services.AddScoped<UpdateProductValidator<Product>>();
        services.AddScoped<DeleteProductValidator<Product>>();

        services.DiscoverServices();
        return services;
    }

    internal static IServiceCollection AddMapsterConfig(this IServiceCollection services)
    {
        var globalSettings = TypeAdapterConfig.GlobalSettings;

        globalSettings.Scan(Assembly.GetExecutingAssembly());
        services.AddSingleton(globalSettings);
        services.AddScoped<IMapper, ServiceMapper>();
        
        return services;
    }
}