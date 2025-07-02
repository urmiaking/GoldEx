using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Services;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class ReportingService(IInvoiceRepository invoiceRepository, ISettingService settingService, IMapper mapper) : IReportingService
{
    public async Task<GetInvoiceReportResponse> GetInvoiceReportAsync(long invoiceNumber, CancellationToken cancellationToken = default)
    {
        var invoiceResponse = await GetInvoiceDetailAsync(invoiceNumber, cancellationToken);
        var setting = await settingService.GetAsync(cancellationToken);

        return new GetInvoiceReportResponse(invoiceResponse, setting!);
    }

    private async Task<GetInvoiceDetailResponse> GetInvoiceDetailAsync(long invoiceNumber, CancellationToken cancellationToken = default)
    {
        var item = await invoiceRepository
            .Get(new InvoicesByNumberSpecification(invoiceNumber))
            .Include(x => x.Customer)
            .ThenInclude(x => x.CreditLimitPriceUnit)
            .Include(x => x.PriceUnit)
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .ThenInclude(x => x!.ProductCategory)
            .Include(x => x.InvoicePayments)
            .ThenInclude(x => x.PriceUnit)
            .Include(x => x.InvoicePayments)
            .ThenInclude(x => x.PaymentMethod)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetInvoiceDetailResponse>(item);
    }
}