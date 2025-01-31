using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Server.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        //services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.DiscoverServices();
        return services;
    }
}