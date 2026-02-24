using GoldEx.Client.Components;
using GoldEx.Client.Services;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using MudBlazor;
using System.Runtime.InteropServices;

namespace GoldEx.Client.Extensions;

public static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.DiscoverServices();

        return services;
    }

    public static IServiceCollection AddClientServerServices(this IServiceCollection services)
    {
        if (RuntimeInformation.OSArchitecture == Architecture.Wasm)
            services.AddClientServices();

        services.AddClientAndServerServices(Defaults.Classes.Position.BottomLeft);

        return services;
    }
}