using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Services.Abstractions;

public interface IInvoiceService
{
    Task CreateAsync(InvoiceRequestDto request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, InvoiceRequestDto request, CancellationToken cancellationToken = default);
    Task<PagedList<GetInvoiceListResponse>> GetListAsync(RequestFilter filter, InvoiceFilter invoiceFilter, 
        Guid? customerId, CancellationToken cancellationToken = default);
    Task<List<GetTinyInvoiceResponse>> GetCustomerInvoicesAsync(Guid customerId, RequestFilter filter, CancellationToken cancellationToken = default);
    Task<GetInvoiceResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetInvoiceResponse> GetAsync(long invoiceNumber, InvoiceType invoiceType, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetInvoiceNumberResponse> GetLastNumberAsync(InvoiceType invoiceType, CancellationToken cancellationToken = default);
    Task SendReminderAsync(Guid id, CancellationToken cancellationToken = default);
}