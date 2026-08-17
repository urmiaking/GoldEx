using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.StoreAggregate;
using System;

namespace GoldEx.Server.Domain.OAuthAggregate;

public readonly record struct OAuthTokenId(Guid Value);

public class OAuthToken : EntityBase<OAuthTokenId>, IStoreFiltered
{
    public static OAuthToken Create(
        string accessTokenHash,
        string? refreshTokenHash,
        string clientId,
        Guid userId,
        StoreId storeId,
        string scope,
        TimeSpan accessTokenLifetime,
        TimeSpan? refreshTokenLifetime = null)
    {
        var now = DateTimeOffset.UtcNow;
        return new OAuthToken
        {
            Id = new OAuthTokenId(Guid.CreateVersion7()),
            AccessTokenHash = accessTokenHash,
            RefreshTokenHash = refreshTokenHash,
            ClientId = clientId,
            UserId = userId,
            StoreId = storeId,
            Scope = scope,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = now.Add(accessTokenLifetime),
            RefreshTokenExpiresAt = refreshTokenLifetime.HasValue ? now.Add(refreshTokenLifetime.Value) : now.AddDays(30),
            IsRevoked = false
        };
    }

#pragma warning disable CS8618
    private OAuthToken() { }
#pragma warning restore CS8618

    public string AccessTokenHash { get; private set; }
    public string? RefreshTokenHash { get; private set; }
    public string ClientId { get; private set; }
    public Guid UserId { get; private set; }
    public StoreId StoreId { get; private set; }
    public string Scope { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RefreshTokenExpiresAt { get; private set; }
    public DateTimeOffset? LastUsedAt { get; private set; }
    public bool IsRevoked { get; private set; }

    public bool IsAccessTokenExpired => ExpiresAt < DateTimeOffset.UtcNow;
    public bool IsRefreshTokenExpired => RefreshTokenExpiresAt.HasValue && RefreshTokenExpiresAt.Value < DateTimeOffset.UtcNow;
    public bool IsActive => !IsRevoked && !IsAccessTokenExpired;

    public void Revoke()
    {
        IsRevoked = true;
    }

    public void RecordUsage()
    {
        LastUsedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateAccessToken(string newAccessTokenHash, TimeSpan lifetime)
    {
        AccessTokenHash = newAccessTokenHash;
        ExpiresAt = DateTimeOffset.UtcNow.Add(lifetime);
        IsRevoked = false;
    }
}
