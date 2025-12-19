using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CoinInstanceAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal sealed class CoinInstanceRepository(GoldExDbContext dbContext) : RepositoryBase<CoinInstance>(dbContext), ICoinInstanceRepository;