using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.PersonalAccessTokenAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal sealed class PersonalAccessTokenRepository(GoldExDbContext dbContext)
    : RepositoryBase<PersonalAccessToken>(dbContext), IPersonalAccessTokenRepository;
