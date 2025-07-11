using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Invoices;

namespace GoldEx.Shared.Services.Abstractions;

public interface IInvoiceService
{
    Task SetAsync(InvoiceRequestDto request, CancellationToken cancellationToken = default);
    Task<PagedList<GetInvoiceListResponse>> GetListAsync(RequestFilter filter, Guid? customerId, CancellationToken cancellationToken = default);
    Task<GetInvoiceResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetInvoiceResponse> GetAsync(long invoiceNumber, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, bool deleteProducts, CancellationToken cancellationToken = default);
    Task<GetInvoiceNumberResponse> GetLastNumberAsync(CancellationToken cancellationToken = default);
}