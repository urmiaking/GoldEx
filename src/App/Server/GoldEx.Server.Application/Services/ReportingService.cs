using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class ReportingService(IInvoiceRepository invoiceRepository, ISettingService settingService, IMapper mapper) : IReportingService
{
    public async Task<GetInvoiceReportResponse> GetInvoiceReportAsync(long invoiceNumber, InvoiceType invoiceType,
        CancellationToken cancellationToken = default)
    {
        var invoiceResponse = await GetInvoiceDetailAsync(invoiceNumber, invoiceType, cancellationToken);
        var setting = await settingService.GetAsync(cancellationToken);

        return new GetInvoiceReportResponse(invoiceResponse, setting!);
    }

    private async Task<GetInvoiceDetailResponse> GetInvoiceDetailAsync(long invoiceNumber, InvoiceType invoiceType, CancellationToken cancellationToken = default)
    {
        var item = await invoiceRepository
            .Get(new InvoicesByNumberSpecification(invoiceNumber, invoiceType))
            .Include(x => x.Customer!)
                .ThenInclude(x => x.CreditLimitPriceUnit)
            .Include(x => x.PriceUnit)
            .Include(x => x.ProductItems)
                .ThenInclude(x => x.SellProduct)
                    .ThenInclude(x => x!.ProductCategory)
            .Include(x => x.ProductItems)
                .ThenInclude(x => x.PurchaseProduct)
                    .ThenInclude(x => x!.ProductCategory)
            .Include(x => x.InvoicePayments)
                .ThenInclude(x => x.PriceUnit)
            .Include(x => x.UnpaidPriceUnit)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetInvoiceDetailResponse>(item);
    }
}