using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Shared.DTOs.Invoices;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IServerInvoicePaymentService
{
    Task SyncPaymentsWithInvoiceAsync(Invoice invoice, List<InvoicePaymentDto> invoicePayments, CancellationToken cancellationToken = default);
}