using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class InvoiceRepository(GoldExDbContext dbContext) : RepositoryBase<Invoice>(dbContext), IInvoiceRepository
{
    public async Task<long> GetLastNumberAsync(InvoiceType invoiceType, CancellationToken cancellationToken = default)
    {
        var invoiceNumber = await Query
            .Where(x => x.InvoiceType == invoiceType)
            .OrderByDescending(x => x.InvoiceNumber)
            .Select(x => x.InvoiceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        return invoiceNumber;
    }

    public async Task<List<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default)
    {
        return await Query
            .Include(x => x.PriceUnit)
            .Include(x => x.Customer)
            .Where(x => x.DueDate < DateOnly.FromDateTime(DateTime.Now) &&
                        !x.Notifications!.Any())
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}