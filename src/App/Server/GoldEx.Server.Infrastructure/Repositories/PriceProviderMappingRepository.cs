using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.PriceProviderMappingAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal sealed class PriceProviderMappingRepository(GoldExDbContext dbContext) : RepositoryBase<PriceProviderMapping>(dbContext), IPriceProviderMappingRepository;