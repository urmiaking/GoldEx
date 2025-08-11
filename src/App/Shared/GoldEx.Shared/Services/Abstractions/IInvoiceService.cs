using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Services.Abstractions;

public interface IInvoiceService
{
    Task CreateAsync(InvoiceRequestDto request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, InvoiceRequestDto requestDto, CancellationToken cancellationToken = default);
    Task<PagedList<GetInvoiceListResponse>> GetListAsync(RequestFilter filter, InvoiceFilter invoiceFilter, 
        Guid? customerId, CancellationToken cancellationToken = default);
    Task<GetInvoiceResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetInvoiceResponse> GetAsync(long invoiceNumber, InvoiceType invoiceType, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, bool deleteProducts, CancellationToken cancellationToken = default);
    Task<GetInvoiceNumberResponse> GetLastNumberAsync(InvoiceType invoiceType, CancellationToken cancellationToken = default);
}