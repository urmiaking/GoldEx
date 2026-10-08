using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.SmartTrayAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class SmartTrayRepository(GoldExDbContext dbContext) : RepositoryBase<SmartTray>(dbContext), ISmartTrayRepository
{
    public async Task<List<SmartTray>> GetActiveTraysAsync(CancellationToken cancellationToken = default)
    {
        return await Query
            .Where(x => x.Status == SmartTrayStatus.Active || x.Status == SmartTrayStatus.Auditing)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<SmartTray?> GetByTrayNumberAsync(int trayNumber, CancellationToken cancellationToken = default)
    {
        return await Query
            .FirstOrDefaultAsync(x => x.TrayNumber == trayNumber, cancellationToken);
    }

    public async Task<int> GetNextTrayNumberAsync(CancellationToken cancellationToken = default)
    {
        var maxNumber = await Query
            .Select(x => (int?)x.TrayNumber)
            .MaxAsync(cancellationToken);

        return (maxNumber ?? 0) + 1;
    }

    public async Task<bool> IsItemInAnyActiveTrayAsync(string barcode, Guid? excludeTrayId = null, CancellationToken cancellationToken = default)
    {
        var normalized = barcode.Trim();

        var query = dbContext.SmartTrayItems
            .Where(item => item.Barcode == normalized && item.Status == SmartTrayItemStatus.InTray)
            .Where(item => item.SmartTray!.Status == SmartTrayStatus.Active || item.SmartTray!.Status == SmartTrayStatus.Auditing);

        if (excludeTrayId.HasValue)
        {
            var excludeId = new SmartTrayId(excludeTrayId.Value);
            query = query.Where(item => item.SmartTrayId != excludeId);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<SmartTray?> GetWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var trayId = new SmartTrayId(id);
        return await Query
            .FirstOrDefaultAsync(x => x.Id == trayId, cancellationToken);
    }
}
