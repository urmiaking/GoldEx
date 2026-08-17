using System;

namespace GoldEx.Shared.DTOs.PersonalAccessTokens;

public class CreatePersonalAccessTokenResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RawToken { get; set; } = string.Empty;
    public string TokenPrefix { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}
