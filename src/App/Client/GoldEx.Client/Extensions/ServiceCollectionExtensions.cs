using GoldEx.Client.Components;
using GoldEx.Client.Services;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using MudBlazor;
using System.Runtime.InteropServices;

namespace GoldEx.Client.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddServices()
        {
            services.DiscoverServices();

            return services;
        }

        public IServiceCollection AddClientServerServices()
        {
            if (RuntimeInformation.OSArchitecture == Architecture.Wasm)
                services.AddClientServices();

            services.AddClientAndServerServices(Defaults.Classes.Position.BottomLeft);

            return services;
        }
    }
}