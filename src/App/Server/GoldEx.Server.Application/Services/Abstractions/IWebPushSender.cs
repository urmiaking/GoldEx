namespace GoldEx.Server.Application.Services.Abstractions;

public sealed record WebPushMessage(string Title, string Body, string Url, string Tag);
public sealed record WebPushResult(string Outcome, int? StatusCode = null);
public interface IWebPushSender
{
    bool IsConfigured { get; }
    string? PublicKey { get; }
    Task<WebPushResult> SendAsync(string endpoint, string p256dh, string auth, WebPushMessage message, CancellationToken ct);
}
