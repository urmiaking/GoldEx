using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Server.Extensions;

namespace GoldEx.Server;

public static class DependencyInjection
{
    internal static IServiceCollection AddServer(this IServiceCollection services, IConfiguration configuration)
    {
        services.InitializeDefaultCulture()
            .AddControllers(configuration)
            //.AddClient(configuration)
            .AddComponents()
            .AddStorage(configuration)
            .AddAuth()
            .AddServices()
            .AddSwagger()
            .AddMapster()
            .AddCache();

        services.DiscoverServices();
        return services;
    }
}