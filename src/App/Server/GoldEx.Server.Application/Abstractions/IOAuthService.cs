using GoldEx.Server.Domain.OAuthAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Shared.DTOs.OAuth;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GoldEx.Server.Application.Abstractions;

public interface IOAuthService
{
    Task<ClientRegistrationResponse> RegisterClientAsync(ClientRegistrationRequest request, CancellationToken cancellationToken = default);
    Task<OAuthClient?> GetClientAsync(string clientId, CancellationToken cancellationToken = default);
    Task<OAuthAuthorizationCode> CreateAuthorizationCodeAsync(
        string clientId,
        Guid userId,
        StoreId storeId,
        string redirectUri,
        string? scope,
        string? codeChallenge,
        string? codeChallengeMethod,
        CancellationToken cancellationToken = default);
    Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(OAuthTokenRequest request, CancellationToken cancellationToken = default);
    Task<OAuthTokenResponse> RefreshTokenAsync(string refreshToken, string? clientId, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<OAuthToken?> ValidateAccessTokenAsync(string rawAccessToken, CancellationToken cancellationToken = default);
}
