using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class InvoiceRepository(GoldExDbContext dbContext) : RepositoryBase<Invoice>(dbContext), IInvoiceRepository
{
    public async Task<long> GetLastNumberAsync(CancellationToken cancellationToken = default)
    {
        var invoiceNumber = await Query
            .OrderByDescending(x => x.InvoiceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        return invoiceNumber?.InvoiceNumber ?? 0;
    }
}