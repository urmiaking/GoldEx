using GoldEx.Client.Abstractions.Common;
using GoldEx.Sdk.Common.DependencyInjections;
using Microsoft.JSInterop;

namespace GoldEx.Client.Components.Services;

[ScopedService]
internal class NetworkStatusService(IJSRuntime jsRuntime) : INetworkStatusService
{
    public async Task<bool> IsOnlineAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await jsRuntime.InvokeAsync<bool>(
                "eval", cancellationToken, "navigator.onLine;" 
            );
        }
        catch (JSException ex)
        {
            Console.WriteLine(@$"Error getting online status: {ex.Message}");
            return false;
        }
    }
}