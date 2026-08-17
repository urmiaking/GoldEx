using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.StoreAggregate;
using System;

namespace GoldEx.Server.Domain.PersonalAccessTokenAggregate;

public readonly record struct PersonalAccessTokenId(Guid Value);

public class PersonalAccessToken : EntityBase<PersonalAccessTokenId>, IStoreFiltered
{
    public static PersonalAccessToken Create(
        Guid userId,
        string name,
        string tokenHash,
        string tokenPrefix,
        DateTimeOffset? expiresAt = null,
        StoreId storeId = default)
    {
        return new PersonalAccessToken
        {
            Id = new PersonalAccessTokenId(Guid.CreateVersion7()),
            UserId = userId,
            Name = name,
            TokenHash = tokenHash,
            TokenPrefix = tokenPrefix,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsRevoked = false,
            StoreId = storeId
        };
    }

#pragma warning disable CS8618
    private PersonalAccessToken() { }
#pragma warning restore CS8618

    public Guid UserId { get; private set; }
    public StoreId StoreId { get; private set; }
    public string Name { get; private set; }
    public string TokenHash { get; private set; }
    public string TokenPrefix { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public DateTimeOffset? LastUsedAt { get; private set; }
    public bool IsRevoked { get; private set; }

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTimeOffset.UtcNow;
    public bool IsActive => !IsRevoked && !IsExpired;

    public void Revoke()
    {
        IsRevoked = true;
    }

    public void RecordUsage()
    {
        LastUsedAt = DateTimeOffset.UtcNow;
    }
}
