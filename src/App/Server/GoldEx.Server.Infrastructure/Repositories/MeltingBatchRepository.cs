using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.MeltingBatchAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class MeltingBatchRepository(GoldExDbContext dbContext) : RepositoryBase<MeltingBatch>(dbContext), IMeltingBatchRepository;