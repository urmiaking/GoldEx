﻿using GoldEx.Server.Extensions;
using GoldEx.Client.Extensions;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;

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
            .AddSettings(configuration)
            .AddAppHealthCheck(configuration)
            .AddSerilogUiService(configuration);

        services.DiscoverServices();
        return services;
    }
}