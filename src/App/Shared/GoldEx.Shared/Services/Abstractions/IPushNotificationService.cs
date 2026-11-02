using GoldEx.Shared.DTOs.Notifications;

namespace GoldEx.Shared.Services.Abstractions;

public interface IPushNotificationService
{
    Task<PushConfigurationResponse> GetConfigurationAsync(CancellationToken ct = default);
    Task RegisterAsync(RegisterPushRequest request, CancellationToken ct = default);
    Task RevokeAsync(RevokePushRequest request, CancellationToken ct = default);
    Task<PushTestResponse> SendTestAsync(RevokePushRequest request, CancellationToken ct = default);
}
