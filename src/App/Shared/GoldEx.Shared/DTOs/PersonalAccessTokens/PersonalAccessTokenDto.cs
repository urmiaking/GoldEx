using System;

namespace GoldEx.Shared.DTOs.PersonalAccessTokens;

public class PersonalAccessTokenDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TokenPrefix { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTimeOffset.UtcNow;
    public bool IsActive => !IsRevoked && !IsExpired;
}
