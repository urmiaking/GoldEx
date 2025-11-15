using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.InventoryEntryAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IInventoryEntryRepository : IRepository<InventoryEntry>,
    ICreateRepository<InventoryEntry>;