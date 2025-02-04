using GoldEx.Server.Extensions;
using GoldEx.Client.Extensions;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Client.Components;

namespace GoldEx.Server;

public static class DependencyInjection
{
    internal static IServiceCollection AddServer(this IServiceCollection services, IConfiguration configuration)
    {
        services.InitializeDefaultCulture()
            .AddControllers(configuration)
            .AddComponents()
            .AddStorage(configuration)
            .AddAuth()
            .AddServices()
            .AddSwagger()
            .AddMapster()
            .AddCache();

        services.AddScoped<IThemeService, ThemeService>();

        services.DiscoverServices();
        return services;
    }
}