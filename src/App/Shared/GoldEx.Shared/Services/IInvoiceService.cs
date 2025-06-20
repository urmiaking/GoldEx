using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Invoices;

namespace GoldEx.Shared.Services;

public interface IInvoiceService
{
    Task CreateAsync(InvoiceRequestDto request, CancellationToken cancellationToken = default);
    Task<PagedList<GetInvoiceResponse>> GetListAsync(RequestFilter filter, Guid? customerId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}