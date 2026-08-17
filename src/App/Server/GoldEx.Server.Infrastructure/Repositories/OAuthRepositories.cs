using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.OAuthAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal sealed class OAuthClientRepository(GoldExDbContext dbContext)
    : RepositoryBase<OAuthClient>(dbContext), IOAuthClientRepository;

[ScopedService]
internal sealed class OAuthAuthorizationCodeRepository(GoldExDbContext dbContext)
    : RepositoryBase<OAuthAuthorizationCode>(dbContext), IOAuthAuthorizationCodeRepository;

[ScopedService]
internal sealed class OAuthTokenRepository(GoldExDbContext dbContext)
    : RepositoryBase<OAuthToken>(dbContext), IOAuthTokenRepository;
