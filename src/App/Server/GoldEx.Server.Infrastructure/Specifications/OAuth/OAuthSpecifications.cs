using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.OAuthAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.OAuth;

public class OAuthClientByClientIdSpecification(string clientId)
    : SpecificationBase<OAuthClient>(x => x.ClientId == clientId && x.IsActive);

public class OAuthAuthorizationCodeByCodeSpecification(string code)
    : SpecificationBase<OAuthAuthorizationCode>(x => x.Code == code && !x.IsUsed);

public class OAuthTokenByAccessTokenHashSpecification(string hash)
    : SpecificationBase<OAuthToken>(x => x.AccessTokenHash == hash && !x.IsRevoked);

public class OAuthTokenByRefreshTokenHashSpecification(string hash)
    : SpecificationBase<OAuthToken>(x => x.RefreshTokenHash == hash && !x.IsRevoked);
