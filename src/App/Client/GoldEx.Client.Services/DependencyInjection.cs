using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Client.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddClientServices(this IServiceCollection services)
    {
        services.DiscoverServices();

        return services;
    }
}