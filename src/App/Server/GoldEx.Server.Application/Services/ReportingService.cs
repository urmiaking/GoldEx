using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Services;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class ReportingService(IInvoiceService invoiceService, ISettingService settingService) : IReportingService
{
    public async Task<GetInvoiceReportResponse> GetInvoiceReportAsync(long invoiceNumber, CancellationToken cancellationToken = default)
    {
        var invoiceResponse = await invoiceService.GetAsync(invoiceNumber, cancellationToken);
        var setting = await settingService.GetAsync(cancellationToken);

        return new GetInvoiceReportResponse(invoiceResponse, setting!);
    }
}