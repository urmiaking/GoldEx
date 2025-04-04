﻿using GoldEx.Client.Components;
using GoldEx.Client.Extensions;
using GoldEx.Client.Offline;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace GoldEx.Client;

internal static class DependencyInjection
{
    internal static IServiceCollection AddClient(this IServiceCollection services, IWebAssemblyHostEnvironment environment)
    {
        services.InitializeDefaultCulture()
            .AddClientComponents()
            .AddClientServerServices()
            .AddAuthServices()
            .AddServices()
            .AddJsonOptions()
            .AddMapsterConfig()
            .AddHttpClientService(environment)
            .AddOfflineClient();

        return services;
    }
}