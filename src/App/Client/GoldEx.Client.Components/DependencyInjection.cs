using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Client.Components;

public static class DependencyInjection
{
    public static IServiceCollection AddClientComponents(this IServiceCollection services)
    {
        return services.DiscoverServices();
    }
}