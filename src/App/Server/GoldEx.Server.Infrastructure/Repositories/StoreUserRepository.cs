using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class StoreUserRepository(GoldExDbContext dbContext)
    : RepositoryBase<StoreUser>(dbContext), IStoreUserRepository;
