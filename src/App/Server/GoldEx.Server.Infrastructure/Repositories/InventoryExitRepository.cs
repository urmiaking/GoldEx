using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.InventoryExitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class InventoryExitRepository(GoldExDbContext dbContext) : RepositoryBase<InventoryExit>(dbContext), IInventoryExitRepository;