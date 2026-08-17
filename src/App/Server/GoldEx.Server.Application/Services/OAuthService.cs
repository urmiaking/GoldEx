using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Abstractions;
using GoldEx.Server.Domain.OAuthAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.OAuth;
using GoldEx.Shared.DTOs.OAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal sealed class OAuthService(
    IOAuthClientRepository clientRepository,
    IOAuthAuthorizationCodeRepository authCodeRepository,
    IOAuthTokenRepository tokenRepository) : IOAuthService
{
    private const string AccessTokenPrefix = "gex_at_";
    private const string RefreshTokenPrefix = "gex_rt_";
    private const string ClientIdPrefix = "gex_cli_";
    private const string ClientSecretPrefix = "gex_sec_";
    private const string AuthCodePrefix = "gex_code_";

    public async Task<ClientRegistrationResponse> RegisterClientAsync(ClientRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var rawClientId = ClientIdPrefix + GenerateRandomToken(16);
        var rawClientSecret = ClientSecretPrefix + GenerateRandomToken(32);
        var secretHash = HashString(rawClientSecret);

        var redirectUris = request.RedirectUris is { Count: > 0 } ? request.RedirectUris : ["https://oauth.pstmn.io/v1/callback"];
        var grantTypes = request.GrantTypes is { Count: > 0 } ? request.GrantTypes : ["authorization_code", "refresh_token"];
        var responseTypes = request.ResponseTypes is { Count: > 0 } ? request.ResponseTypes : ["code"];

        var client = OAuthClient.Create(
            clientId: rawClientId,
            clientName: request.ClientName ?? "AI Assistant Client",
            clientSecretHash: secretHash,
            redirectUrisJson: JsonSerializer.Serialize(redirectUris),
            grantTypesJson: JsonSerializer.Serialize(grantTypes));

        await clientRepository.CreateAsync(client, cancellationToken);

        return new ClientRegistrationResponse
        {
            ClientId = client.ClientId,
            ClientSecret = rawClientSecret,
            ClientName = client.ClientName,
            RedirectUris = redirectUris,
            GrantTypes = grantTypes,
            ResponseTypes = responseTypes,
            TokenEndpointAuthMethod = request.TokenEndpointAuthMethod ?? "client_secret_post",
            ClientIdIssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    public async Task<OAuthClient?> GetClientAsync(string clientId, CancellationToken cancellationToken = default)
    {
        return await clientRepository
            .Get(new OAuthClientByClientIdSpecification(clientId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<OAuthAuthorizationCode> CreateAuthorizationCodeAsync(
        string clientId,
        Guid userId,
        StoreId storeId,
        string redirectUri,
        string? scope,
        string? codeChallenge,
        string? codeChallengeMethod,
        CancellationToken cancellationToken = default)
    {
        var rawCode = AuthCodePrefix + GenerateRandomToken(32);

        var authCode = OAuthAuthorizationCode.Create(
            code: rawCode,
            clientId: clientId,
            userId: userId,
            storeId: storeId,
            redirectUri: redirectUri,
            scope: scope,
            codeChallenge: codeChallenge,
            codeChallengeMethod: codeChallengeMethod,
            lifetime: TimeSpan.FromMinutes(10));

        await authCodeRepository.CreateAsync(authCode, cancellationToken);
        return authCode;
    }

    public async Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(OAuthTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new InvalidOperationException("کد احراز هویت (code) الزامی است.");

        var authCode = await authCodeRepository
            .Get(new OAuthAuthorizationCodeByCodeSpecification(request.Code))
            .FirstOrDefaultAsync(cancellationToken);

        if (authCode == null || !authCode.IsValid)
            throw new InvalidOperationException("کد احراز هویت نامعتبر، منقضی شده یا قبلاً استفاده شده است.");

        // Validate client_id if provided
        if (!string.IsNullOrWhiteSpace(request.ClientId) &&
            !string.Equals(authCode.ClientId, request.ClientId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("شناسه کلاینت (client_id) با کد تایید مطابقت ندارد.");
        }

        // Validate PKCE if challenge was set
        if (!string.IsNullOrWhiteSpace(authCode.CodeChallenge))
        {
            if (string.IsNullOrWhiteSpace(request.CodeVerifier))
                throw new InvalidOperationException("کد اعتبارسنجی PKCE (code_verifier) الزامی است.");

            if (!ValidatePkce(request.CodeVerifier, authCode.CodeChallenge, authCode.CodeChallengeMethod))
                throw new InvalidOperationException("کد اعتبارسنجی PKCE نامعتبر است.");
        }

        // Mark code as used
        authCode.MarkUsed();
        await authCodeRepository.UpdateAsync(authCode, cancellationToken);

        // Issue Access Token & Refresh Token
        var rawAccessToken = AccessTokenPrefix + GenerateRandomToken(32);
        var rawRefreshToken = RefreshTokenPrefix + GenerateRandomToken(32);

        var accessTokenHash = HashString(rawAccessToken);
        var refreshTokenHash = HashString(rawRefreshToken);

        var accessTokenLifetime = TimeSpan.FromDays(30); // 30 days for AI assistant continuity
        var refreshTokenLifetime = TimeSpan.FromDays(90);

        var token = OAuthToken.Create(
            accessTokenHash: accessTokenHash,
            refreshTokenHash: refreshTokenHash,
            clientId: authCode.ClientId,
            userId: authCode.UserId,
            storeId: authCode.StoreId,
            scope: authCode.Scope,
            accessTokenLifetime: accessTokenLifetime,
            refreshTokenLifetime: refreshTokenLifetime);

        await tokenRepository.CreateAsync(token, cancellationToken);

        return new OAuthTokenResponse
        {
            AccessToken = rawAccessToken,
            TokenType = "Bearer",
            ExpiresIn = (int)accessTokenLifetime.TotalSeconds,
            RefreshToken = rawRefreshToken,
            Scope = token.Scope
        };
    }

    public async Task<OAuthTokenResponse> RefreshTokenAsync(string refreshToken, string? clientId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new InvalidOperationException("توکن بازنشانی (refresh_token) الزامی است.");

        var refreshTokenHash = HashString(refreshToken);

        var token = await tokenRepository
            .Get(new OAuthTokenByRefreshTokenHashSpecification(refreshTokenHash))
            .FirstOrDefaultAsync(cancellationToken);

        if (token == null || token.IsRevoked || token.IsRefreshTokenExpired)
            throw new InvalidOperationException("توکن بازنشانی نامعتبر یا منقضی شده است.");

        if (!string.IsNullOrWhiteSpace(clientId) &&
            !string.Equals(token.ClientId, clientId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("شناسه کلاینت مطابقت ندارد.");
        }

        var newRawAccessToken = AccessTokenPrefix + GenerateRandomToken(32);
        var newAccessTokenHash = HashString(newRawAccessToken);
        var lifetime = TimeSpan.FromDays(30);

        token.UpdateAccessToken(newAccessTokenHash, lifetime);
        await tokenRepository.UpdateAsync(token, cancellationToken);

        return new OAuthTokenResponse
        {
            AccessToken = newRawAccessToken,
            TokenType = "Bearer",
            ExpiresIn = (int)lifetime.TotalSeconds,
            RefreshToken = refreshToken,
            Scope = token.Scope
        };
    }

    public async Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return;

        var tokenHash = HashString(token);

        var oauthToken = await tokenRepository
            .Get(new OAuthTokenByAccessTokenHashSpecification(tokenHash))
            .FirstOrDefaultAsync(cancellationToken);

        oauthToken ??= await tokenRepository
            .Get(new OAuthTokenByRefreshTokenHashSpecification(tokenHash))
            .FirstOrDefaultAsync(cancellationToken);

        if (oauthToken != null)
        {
            oauthToken.Revoke();
            await tokenRepository.UpdateAsync(oauthToken, cancellationToken);
        }
    }

    public async Task<OAuthToken?> ValidateAccessTokenAsync(string rawAccessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawAccessToken))
            return null;

        var tokenHash = HashString(rawAccessToken);

        var token = await tokenRepository
            .Get(new OAuthTokenByAccessTokenHashSpecification(tokenHash))
            .FirstOrDefaultAsync(cancellationToken);

        if (token is { IsActive: true })
        {
            token.RecordUsage();
            await tokenRepository.UpdateAsync(token, cancellationToken);
            return token;
        }

        return null;
    }

    private static bool ValidatePkce(string codeVerifier, string codeChallenge, string? method)
    {
        if (string.Equals(method, "plain", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(codeVerifier, codeChallenge, StringComparison.Ordinal);
        }

        // Default: S256
        var hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        var computedChallenge = Base64UrlEncode(hash);
        return string.Equals(computedChallenge, codeChallenge, StringComparison.Ordinal);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string GenerateRandomToken(int byteLength)
    {
        var bytes = new byte[byteLength];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string HashString(string input)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
