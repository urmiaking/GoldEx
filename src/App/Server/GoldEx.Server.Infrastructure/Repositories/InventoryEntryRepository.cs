using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.InventoryEntryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class InventoryEntryRepository(GoldExDbContext dbContext) : RepositoryBase<InventoryEntry>(dbContext), IInventoryEntryRepository;