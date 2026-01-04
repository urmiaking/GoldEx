using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IInvoicePrintService
{
    Task<GetInvoiceReportResponse> GetInvoiceReportAsync(long invoiceNumber, InvoiceType invoiceType, CancellationToken cancellationToken = default);
}