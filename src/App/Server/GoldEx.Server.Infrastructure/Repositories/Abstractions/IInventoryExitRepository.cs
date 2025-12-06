using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.InventoryExitAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IInventoryExitRepository : IRepository<InventoryExit>,
    ICreateRepository<InventoryExit>,
    IUpdateRepository<InventoryExit>,
    IDeleteRepository<InventoryExit>;