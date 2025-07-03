using GoldEx.Client.Offline.Domain.CustomerAggregate;
using Microsoft.Extensions.DependencyInjection;
using GoldEx.Client.Offline.Domain.PriceAggregate;
using GoldEx.Client.Offline.Domain.ProductAggregate;
using GoldEx.Client.Offline.Domain.ProductCategoryAggregate;
using GoldEx.Client.Offline.Domain.SettingsAggregate;
using GoldEx.Client.Offline.Domain.TransactionAggregate;
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
using GoldEx.Shared.Application.Validators.Transactions;

namespace GoldEx.Client.Offline;

public static class DependencyInjection
{
    public static IServiceCollection AddOfflineClient(this IServiceCollection services)
    {
        

        services.DiscoverServices();
        return services;
    }
}