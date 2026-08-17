using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.OAuthAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IOAuthClientRepository : IRepository<OAuthClient>,
    ICreateRepository<OAuthClient>,
    IUpdateRepository<OAuthClient>,
    IDeleteRepository<OAuthClient>;

public interface IOAuthAuthorizationCodeRepository : IRepository<OAuthAuthorizationCode>,
    ICreateRepository<OAuthAuthorizationCode>,
    IUpdateRepository<OAuthAuthorizationCode>,
    IDeleteRepository<OAuthAuthorizationCode>;

public interface IOAuthTokenRepository : IRepository<OAuthToken>,
    ICreateRepository<OAuthToken>,
    IUpdateRepository<OAuthToken>,
    IDeleteRepository<OAuthToken>;
