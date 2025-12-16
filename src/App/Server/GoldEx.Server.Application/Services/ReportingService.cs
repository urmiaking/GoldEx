using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class ReportingService(
    IInvoiceRepository invoiceRepository,
    ITransactionRepository transactionRepository,
    ISettingService settingService,
    IMapper mapper) : IReportingService
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
            .AsNoTracking()
            .Include(x => x.Customer!)
                .ThenInclude(x => x.CreditLimitPriceUnit)
            .Include(x => x.ProductItems)
                .ThenInclude(x => x.Product)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.LedgerAccount!)
                    .ThenInclude(x => x.Customer)
            .Include(x => x.CoinItems)
                .ThenInclude(x => x.Coin)
            .Include(x => x.CurrencyItems)
                .ThenInclude(x => x.Currency)
            .Include(x => x.PriceUnit)
            .Include(x => x.UnpaidPriceUnit)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        var unpaid = item.TotalUnpaidAmount;
        var impact = unpaid * (item.InvoiceType == InvoiceType.Sell ? 1m : -1m); // Sell: + (بدهکار مثبت), Purchase: - (بستانکار منفی)

        // مانده قبلی (قبل از CreatedAt)
        var previousRemaining = await transactionRepository.GetCustomerRemainingListAsync(item.CustomerId, null, item.CreatedAt, cancellationToken);

        // مانده پس از فاکتور (previous + impact)
        var afterRemaining = new Dictionary<PriceUnit, decimal>(previousRemaining);
        afterRemaining[item.PriceUnit!] = afterRemaining.GetValueOrDefault(item.PriceUnit!, 0m) + impact; // مستقیم set

        var previousFormatted = FormatRemaining(previousRemaining, item.PriceUnit!, item.UnpaidPriceUnit, item.UnpaidAmountExchangeRate);
        var afterFormatted = FormatRemaining(afterRemaining, item.PriceUnit!, item.UnpaidPriceUnit, item.UnpaidAmountExchangeRate);

        var mapped = mapper.Map<GetInvoiceDetailResponse>(item);
        mapped = mapped with
        {
            PreviousRemaining = previousFormatted,
            AfterRemaining = afterFormatted,
        };

        return mapped;
    }

    private static string FormatRemaining(
        Dictionary<PriceUnit, decimal> remaining,
        PriceUnit mainUnit,
        PriceUnit? secondaryUnit,
        decimal? exchangeRate)
    {
        var mainAmount = remaining.GetValueOrDefault(mainUnit, 0m);
        var isNegative = mainAmount < 0;

        var absMain = Math.Abs(mainAmount);
        var formatted = absMain.ToCurrencyReportFormat(mainUnit.Title);

        if (secondaryUnit != null && exchangeRate.HasValue)
        {
            var secondaryAmount = absMain * exchangeRate.Value;
            var secondaryFormatted = secondaryAmount.ToCurrencyReportFormat(secondaryUnit.Title);

            formatted += $" ({secondaryFormatted})";
        }

        if (isNegative)
        {
            formatted += " (بس)";
        }

        return formatted;
    }
}