using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.Transactions;

namespace GoldEx.Shared.Services;

public interface IInvoiceService
{
    Task CreateAsync(InvoiceRequestDto request, CancellationToken cancellationToken = default);
    Task<PagedList<GetInvoiceListResponse>> GetListAsync(RequestFilter filter, Guid? customerId, CancellationToken cancellationToken = default);
    Task<GetInvoiceResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, bool deleteProducts, CancellationToken cancellationToken = default);
    Task<GetInvoiceNumberResponse> GetLastNumberAsync(CancellationToken cancellationToken = default);
}