using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.DTOs.Notifications;

public sealed record PushConfigurationResponse(bool Enabled, string? PublicKey);
public sealed record RegisterPushRequest(
    [Required, MaxLength(2048)] string Endpoint,
    [Required, MaxLength(128)] string P256dh,
    [Required, MaxLength(64)] string Auth);
public sealed record RevokePushRequest([Required, MaxLength(2048)] string Endpoint);
public sealed record PushTestResponse(Guid DeliveryId, string Status, int? StatusCode);
