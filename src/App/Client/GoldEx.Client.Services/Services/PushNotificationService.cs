using System.Net.Http.Json;
using System.Text.Json;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.Notifications;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal sealed class PushNotificationService(HttpClient client, JsonSerializerOptions options) : IPushNotificationService
{
    public async Task<PushConfigurationResponse> GetConfigurationAsync(CancellationToken ct = default)
    {
        using var response = await client.GetAsync(ApiUrls.Notifications.PushConfiguration, ct);
        Check(response);
        return await response.Content.ReadFromJsonAsync<PushConfigurationResponse>(options, ct) ?? throw new UnexpectedHttpResponseException();
    }
    public async Task RegisterAsync(RegisterPushRequest request, CancellationToken ct = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.Notifications.PushSubscribe, request, options, ct); Check(response);
    }
    public async Task RevokeAsync(RevokePushRequest request, CancellationToken ct = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.Notifications.PushRevoke, request, options, ct); Check(response);
    }
    public async Task<PushTestResponse> SendTestAsync(RevokePushRequest request, CancellationToken ct = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.Notifications.PushTest, request, options, ct); Check(response);
        return await response.Content.ReadFromJsonAsync<PushTestResponse>(options, ct) ?? throw new UnexpectedHttpResponseException();
    }
    private static void Check(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode) throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }
}
