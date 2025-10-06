using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IInvoiceRepository : IRepository<Invoice>,
    ICreateRepository<Invoice>,
    IUpdateRepository<Invoice>,
    IDeleteRepository<Invoice>
{
    Task<long> GetLastNumberAsync(InvoiceType invoiceType, CancellationToken cancellationToken = default);
    Task<List<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default);
}