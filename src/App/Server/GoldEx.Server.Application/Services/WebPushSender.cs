using System.Net;
using System.Text.Json;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using WebPush;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal sealed class WebPushSender(IConfiguration configuration, IHttpClientFactory clients) : IWebPushSender
{
    public string? PublicKey => configuration["Notifications:Push:PublicKey"];
    public bool IsConfigured => configuration.GetValue<bool>("Notifications:Push:Enabled") &&
        !string.IsNullOrWhiteSpace(PublicKey) && !string.IsNullOrWhiteSpace(configuration["Notifications:Push:PrivateKey"]) &&
        Uri.TryCreate(configuration["Notifications:Push:Subject"], UriKind.Absolute, out var subject) &&
        (subject.Scheme == "mailto" || subject.Scheme == "https");

    public async Task<WebPushResult> SendAsync(string endpoint, string p256dh, string auth, WebPushMessage message, CancellationToken ct)
    {
        if (!IsConfigured) return new("NotConfigured");
        // Endpoint validation also runs at dispatch so stale/imported records cannot bypass it.
        if (!PushSubscriptionValidator.ValidEndpoint(endpoint)) return new("Rejected");
        using var http = clients.CreateClient("GoldExWebPush");
        using var client = new WebPushClient(http);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(20));
        try
        {
            await client.SendNotificationAsync(new WebPush.PushSubscription(endpoint, p256dh, auth),
                JsonSerializer.Serialize(message, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
                new Dictionary<string, object>
                {
                    ["vapidDetails"] = new VapidDetails(configuration["Notifications:Push:Subject"], PublicKey,
                        configuration["Notifications:Push:PrivateKey"]),
                    ["TTL"] = 3600
                }, timeout.Token);
            return new("Accepted");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (WebPushException ex)
        {
            var status = (int)ex.StatusCode;
            return new(ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone ? "Gone" :
                status == 429 || status >= 500 ? "Retry" : "Rejected", status);
        }
        catch (HttpRequestException) { return new("Retry"); }
        catch (OperationCanceledException) { return new("Retry"); }
        catch (Exception ex) when (ex is ArgumentException or System.Security.Cryptography.CryptographicException) { return new("Rejected"); }
    }
}
