using GoldEx.Shared.DTOs.Reporting;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IReportingService
{
    Task<GetInvoiceReportResponse> GetInvoiceReportAsync(long invoiceNumber, CancellationToken cancellationToken = default);
}