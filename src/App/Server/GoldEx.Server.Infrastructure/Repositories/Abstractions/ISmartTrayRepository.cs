using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.SmartTrayAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface ISmartTrayRepository : IRepository<SmartTray>,
    ICreateRepository<SmartTray>,
    IUpdateRepository<SmartTray>,
    IDeleteRepository<SmartTray>
{
    Task<List<SmartTray>> GetActiveTraysAsync(CancellationToken cancellationToken = default);
    Task<SmartTray?> GetByTrayNumberAsync(int trayNumber, CancellationToken cancellationToken = default);
    Task<int> GetNextTrayNumberAsync(CancellationToken cancellationToken = default);
    Task<bool> IsItemInAnyActiveTrayAsync(string barcode, Guid? excludeTrayId = null, CancellationToken cancellationToken = default);
    Task<SmartTray?> GetWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
}
