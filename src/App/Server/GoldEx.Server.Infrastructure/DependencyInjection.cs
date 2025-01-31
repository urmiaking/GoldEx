using System.Reflection;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Server.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.DiscoverServices();
        return services;
    }
}