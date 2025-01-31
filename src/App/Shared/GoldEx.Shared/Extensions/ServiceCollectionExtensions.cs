using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services.DiscoverServices();
        return services;
    }
}