using GoldEx.Server.Extensions;
using GoldEx.Client.Extensions;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Client.Components.Services;
using GoldEx.Shared.Abstractions;
using GoldEx.Shared.Settings;

namespace GoldEx.Server;

public static class DependencyInjection
{
    internal static IServiceCollection AddServer(this IServiceCollection services, IConfiguration configuration)
    {
        services.InitializeDefaultCulture()
            .AddControllers(configuration)
            .AddComponents()
            .AddStorage(configuration)
            .AddAuth(configuration)
            .AddServices()
            .AddSwagger()
            .AddMapster()
            .AddCache()
            .AddSettings(configuration);

        services.AddScoped<IThemeService, ThemeService>();

        services.DiscoverServices();
        return services;
    }
}