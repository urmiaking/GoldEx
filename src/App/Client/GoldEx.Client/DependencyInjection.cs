using GoldEx.Client.Components;
using GoldEx.Client.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace GoldEx.Client;

internal static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddClient(IWebAssemblyHostEnvironment environment)
        {
            services.InitializeDefaultCulture()
                .AddClientComponents()
                .AddClientServerServices()
                .AddAuthServices()
                .AddServices()
                .AddJsonOptions()   
                .AddHttpClientService(environment, new TimeSpan(0, 10, 0));

            return services;
        }
    }
}