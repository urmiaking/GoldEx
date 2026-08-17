using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.StoreAggregate;
using System;

namespace GoldEx.Server.Domain.OAuthAggregate;

public readonly record struct OAuthAuthorizationCodeId(Guid Value);

public class OAuthAuthorizationCode : EntityBase<OAuthAuthorizationCodeId>, IStoreFiltered
{
    public static OAuthAuthorizationCode Create(
        string code,
        string clientId,
        Guid userId,
        StoreId storeId,
        string redirectUri,
        string? scope,
        string? codeChallenge,
        string? codeChallengeMethod,
        TimeSpan lifetime)
    {
        return new OAuthAuthorizationCode
        {
            Id = new OAuthAuthorizationCodeId(Guid.CreateVersion7()),
            Code = code,
            ClientId = clientId,
            UserId = userId,
            StoreId = storeId,
            RedirectUri = redirectUri,
            Scope = scope ?? "mcp",
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = codeChallengeMethod ?? "S256",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.Add(lifetime),
            IsUsed = false
        };
    }

#pragma warning disable CS8618
    private OAuthAuthorizationCode() { }
#pragma warning restore CS8618

    public string Code { get; private set; }
    public string ClientId { get; private set; }
    public Guid UserId { get; private set; }
    public StoreId StoreId { get; private set; }
    public string RedirectUri { get; private set; }
    public string Scope { get; private set; }
    public string? CodeChallenge { get; private set; }
    public string? CodeChallengeMethod { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }

    public bool IsExpired => ExpiresAt < DateTimeOffset.UtcNow;
    public bool IsValid => !IsUsed && !IsExpired;

    public void MarkUsed()
    {
        IsUsed = true;
    }
}
