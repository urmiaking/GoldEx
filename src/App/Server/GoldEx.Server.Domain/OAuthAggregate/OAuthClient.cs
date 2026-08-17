using GoldEx.Sdk.Server.Domain.Entities;
using System;

namespace GoldEx.Server.Domain.OAuthAggregate;

public readonly record struct OAuthClientId(Guid Value);

public class OAuthClient : EntityBase<OAuthClientId>
{
    public static OAuthClient Create(
        string clientId,
        string clientName,
        string? clientSecretHash,
        string redirectUrisJson,
        string grantTypesJson)
    {
        return new OAuthClient
        {
            Id = new OAuthClientId(Guid.CreateVersion7()),
            ClientId = clientId,
            ClientName = clientName,
            ClientSecretHash = clientSecretHash,
            RedirectUrisJson = redirectUrisJson,
            GrantTypesJson = grantTypesJson,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

#pragma warning disable CS8618
    private OAuthClient() { }
#pragma warning restore CS8618

    public string ClientId { get; private set; }
    public string ClientName { get; private set; }
    public string? ClientSecretHash { get; private set; }
    public string RedirectUrisJson { get; private set; }
    public string GrantTypesJson { get; private set; }
    public bool IsActive { get; private set; }

    public void Deactivate()
    {
        IsActive = false;
    }
}
