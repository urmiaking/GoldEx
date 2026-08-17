using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PersonalAccessTokenAggregate;
using System;

namespace GoldEx.Server.Infrastructure.Specifications.PersonalAccessTokens;

public class PersonalAccessTokenByIdSpecification(PersonalAccessTokenId id)
    : SpecificationBase<PersonalAccessToken>(x => x.Id == id);

public class PersonalAccessTokenByHashSpecification(string tokenHash)
    : SpecificationBase<PersonalAccessToken>(x => x.TokenHash == tokenHash && !x.IsRevoked);

public class PersonalAccessTokensByUserIdSpecification(Guid userId)
    : SpecificationBase<PersonalAccessToken>(x => x.UserId == userId);
