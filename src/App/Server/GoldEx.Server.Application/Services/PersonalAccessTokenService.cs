using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Server.Application.Abstractions;
using GoldEx.Server.Domain.PersonalAccessTokenAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PersonalAccessTokens;
using GoldEx.Shared.DTOs.PersonalAccessTokens;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal sealed class PersonalAccessTokenService(
    IPersonalAccessTokenRepository repository,
    IUserContext userContext,
    IStoreContext storeContext) : IPersonalAccessTokenService
{
    private const string TokenPrefix = "gex_pat_";

    public async Task<List<PersonalAccessTokenDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var userId = userContext.GetUserId() ?? throw new UnauthorizedAccessException();

        var tokens = await repository
            .Get(new PersonalAccessTokensByUserIdSpecification(userId))
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PersonalAccessTokenDto
            {
                Id = x.Id.Value,
                Name = x.Name,
                TokenPrefix = x.TokenPrefix,
                CreatedAt = x.CreatedAt,
                ExpiresAt = x.ExpiresAt,
                LastUsedAt = x.LastUsedAt,
                IsRevoked = x.IsRevoked
            })
            .ToListAsync(cancellationToken);

        return tokens;
    }

    public async Task<CreatePersonalAccessTokenResponse> CreateAsync(CreatePersonalAccessTokenRequest request, CancellationToken cancellationToken = default)
    {
        var userId = userContext.GetUserId() ?? throw new UnauthorizedAccessException();
        var storeId = storeContext.StoreId.HasValue ? new StoreId(storeContext.StoreId.Value) : default;

        var rawSecret = GenerateSecureRandomString(32);
        var rawToken = $"{TokenPrefix}{rawSecret}";
        var tokenHash = HashToken(rawToken);
        var displayPrefix = $"{TokenPrefix}{rawSecret[..6]}...";

        DateTimeOffset? expiresAt = request.ExpireDays.HasValue && request.ExpireDays.Value > 0
            ? DateTimeOffset.UtcNow.AddDays(request.ExpireDays.Value)
            : null;

        var token = PersonalAccessToken.Create(
            userId: userId,
            name: request.Name.Trim(),
            tokenHash: tokenHash,
            tokenPrefix: displayPrefix,
            expiresAt: expiresAt,
            storeId: storeId);

        await repository.CreateAsync(token, cancellationToken);

        return new CreatePersonalAccessTokenResponse
        {
            Id = token.Id.Value,
            Name = token.Name,
            RawToken = rawToken,
            TokenPrefix = token.TokenPrefix,
            CreatedAt = token.CreatedAt,
            ExpiresAt = token.ExpiresAt
        };
    }

    public async Task RevokeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = userContext.GetUserId() ?? throw new UnauthorizedAccessException();

        var token = await repository
            .Get(new PersonalAccessTokenByIdSpecification(new PersonalAccessTokenId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        if (token.UserId != userId && !userContext.IsInRole(BuiltinRoles.Administrators) && !userContext.IsInRole(BuiltinRoles.Owners))
            throw new UnauthorizedAccessException();

        token.Revoke();
        await repository.UpdateAsync(token, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = userContext.GetUserId() ?? throw new UnauthorizedAccessException();

        var token = await repository
            .Get(new PersonalAccessTokenByIdSpecification(new PersonalAccessTokenId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        if (token.UserId != userId && !userContext.IsInRole(BuiltinRoles.Administrators) && !userContext.IsInRole(BuiltinRoles.Owners))
            throw new UnauthorizedAccessException();

        await repository.DeleteAsync(token, cancellationToken);
    }

    public async Task<PersonalAccessToken?> ValidateAndRecordUsageAsync(string rawToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            return null;

        var tokenHash = HashToken(rawToken.Trim());

        var token = await repository
            .Get(new PersonalAccessTokenByHashSpecification(tokenHash))
            .FirstOrDefaultAsync(cancellationToken);

        if (token == null || !token.IsActive)
            return null;

        token.RecordUsage();
        await repository.UpdateAsync(token, cancellationToken);

        return token;
    }

    public static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string GenerateSecureRandomString(int byteCount)
    {
        var bytes = new byte[byteCount];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
