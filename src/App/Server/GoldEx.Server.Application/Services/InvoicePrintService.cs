using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Reporting;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.InvoiceAggregate;
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
internal class InvoicePrintService(
    IInvoiceRepository invoiceRepository,
    ITransactionRepository transactionRepository,
    ISettingService settingService,
    IMapper mapper) : IInvoicePrintService
{
    public async Task<GetInvoiceReportResponse> GetInvoiceReportAsync(long invoiceNumber, InvoiceType invoiceType,
        CancellationToken cancellationToken = default)
    {
        var invoiceResponse = await GetInvoiceDetailAsync(invoiceNumber, invoiceType, cancellationToken);
        var setting = await settingService.GetAsync(cancellationToken);

        return new GetInvoiceReportResponse(invoiceResponse, setting!);
    }

    public async Task<InvoicePrintDocument?> GetPrintDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.Get(new InvoicesByIdSpecification(new InvoiceId(id)))
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.PriceUnit)
            .Include(x => x.UnpaidPriceUnit)
            .Include(x => x.ProductItems).ThenInclude(x => x.Product)
            .Include(x => x.CoinItems).ThenInclude(x => x.CoinInstance).ThenInclude(x => x!.Coin)
            .Include(x => x.CurrencyItems).ThenInclude(x => x.Currency)
            .Include(x => x.UsedProducts).ThenInclude(x => x.Product)
            .Include(x => x.InvoicePayments!).ThenInclude(x => x.PriceUnit)
            .Include(x => x.InvoicePayments!).ThenInclude(x => x.CheckPayment)
            .Include(x => x.InvoicePayments!).ThenInclude(x => x.CoinInstance).ThenInclude(x => x!.Coin)
            .FirstOrDefaultAsync(cancellationToken);

        if (invoice is null)
            return null;

        var unit = invoice.PriceUnit?.Title ?? "";
        var items = invoice.ProductItems.Select(x => new InvoicePrintLine(
                x.Product?.Name ?? "کالا", x.Quantity, x.ItemFinalAmount + x.ItemStoneAmount,
                ProductType: x.Product?.ProductType, Barcode: x.Product?.Barcode,
                Weight: x.TotalWeight, WeightUnit: x.Product?.GoldUnitType, Fineness: x.Product?.Fineness,
                AssayNumber: x.Product?.MoltenGold?.AssayNumber))
            .Concat(invoice.CoinItems.Select(x => new InvoicePrintLine(
                x.CoinInstance?.Coin?.Title ?? "سکه", x.Quantity, x.ItemFinalAmount,
                Barcode: x.CoinInstance?.Barcode,
                Weight: x.CoinInstance?.Weight * x.Quantity, WeightUnit: GoldUnitType.Gram,
                Fineness: x.CoinInstance?.Fineness, MintYear: x.CoinInstance?.MintYear)))
            .Concat(invoice.CurrencyItems.Select(x => new InvoicePrintLine(
                x.Currency?.Title ?? "ارز", x.Amount, x.ItemFinalAmount,
                QuantityUnit: x.Currency?.Title)))
            .ToArray();

        var usedGold = invoice.UsedProducts.Select(x => new InvoicePrintLine(
            x.Description, x.Quantity, x.ItemFinalAmount,
            ProductType: x.ProductType, Barcode: x.Product?.Barcode,
            Weight: x.Weight * x.Quantity, WeightUnit: x.UnitType, Fineness: x.Product?.Fineness,
            FinenessDeduction: x.FinenessDeductionRate,
            AssayNumber: x.Product?.MoltenGold?.AssayNumber)).ToArray();

        var payments = invoice.InvoicePayments?.OrderBy(x => x.PaymentDate).ThenBy(x => x.CreatedAt)
            .Select(x => new InvoicePrintPayment(
                DateOnly.FromDateTime(x.PaymentDate), x.PaymentType, x.PaymentSide,
                x.FinalAmount, x.PriceUnit?.Title ?? unit, x.FinalAmount * (x.ExchangeRate ?? 1),
                x.CheckPayment?.Number ?? x.ReferenceNumber,
                x.CheckPayment is null ? null : DateOnly.FromDateTime(x.CheckPayment.DueDate),
                x.GoldFineness, x.CoinInstance?.Coin?.Title, x.CoinQuantity)).ToArray() ?? [];

        var goldRates = invoice.ProductItems
            .Where(x => x.Product is not null)
            .Select(x => new InvoicePrintGoldRate(x.GramPrice, x.Product!.GoldUnitType))
            .Concat(invoice.UsedProducts.Select(x => new InvoicePrintGoldRate(x.GramPrice, x.UnitType)))
            .Where(rate => rate.Amount > 0)
            .Distinct()
            .Take(2)
            .ToArray();
        var customer = new InvoicePrintParty(invoice.Customer?.FullName ?? "مشتری", invoice.Customer?.PhoneNumber,
            invoice.Customer?.Address, invoice.Customer?.NationalId);
        var remaining = invoice.TotalUnpaidAmount;
        return new InvoicePrintDocument(
            invoice.InvoiceNumber, invoice.InvoiceType, invoice.TradeScale, invoice.InvoiceDate, invoice.DueDate,
            customer, unit, items, usedGold, payments,
            invoice.TotalAmount, invoice.TotalDiscountAmount, invoice.TotalExtraCostAmount,
            invoice.TotalAmountWithDiscountsAndExtraCosts, invoice.TotalUsedProductsAmount,
            invoice.TotalReceivedFromCustomerAmount, invoice.TotalPaidToCustomerAmount, remaining,
            invoice.UnpaidPriceUnit is null ? null : remaining * invoice.UnpaidAmountExchangeRate,
            invoice.UnpaidPriceUnit?.Title,
            GetPrintCharges(invoice),
            goldRates.Length == 1 ? goldRates[0] : null);
    }

    private static InvoicePrintCharges GetPrintCharges(Invoice invoice)
    {
        IReadOnlyList<InvoiceProductItem> soldProducts = invoice.InvoiceType == InvoiceType.Sell ? invoice.ProductItems : [];
        var profitRates = soldProducts.Where(item => item.Product?.ProductType is not ProductType.UsedGold)
            .Select(item => item.ProfitPercent)
            .Concat(invoice.CoinItems.Select(item => item.ProfitPercent))
            .Concat(invoice.CurrencyItems.Select(item => item.ProfitPercent))
            .Distinct().Order().ToArray();
        var taxRates = soldProducts.Where(item => item.Product?.ProductType is not (ProductType.UsedGold or ProductType.MoltenGold))
            .Select(item => item.TaxPercent)
            .Concat(invoice.CurrencyItems.Select(item => item.TaxPercent))
            .Distinct().Order().ToArray();

        return new InvoicePrintCharges(invoice.TotalWageAmount, profitRates, taxRates, invoice.TotalStoneAmount);
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
            .Include(x => x.ProductItems)
                .ThenInclude(x => x.CostPriceUnit)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.SourceFinancialAccount!)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.LedgerAccount!)
                    .ThenInclude(x => x.Customer)
            .Include(x => x.CoinItems)
                .ThenInclude(x => x.CoinInstance!)
                    .ThenInclude(x => x.Coin)
            .Include(x => x.CoinItems)
                .ThenInclude(x => x.CoinInstance!)
                    .ThenInclude(x => x.CoinInstancePackage!.Issuer)
            .Include(x => x.CurrencyItems)
                .ThenInclude(x => x.Currency)
            .Include(x => x.UsedProducts)
                .ThenInclude(x => x.Product)
            .Include(x => x.PriceUnit!.Price!)
            .Include(x => x.CurrencyItems)
                .ThenInclude(x => x.FinancialAccount)
            .Include(x => x.ProductItems)
                .ThenInclude(x => x.Product!.MoltenGold!.Assayer)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.SourcePayment!.Invoice!)
                    .ThenInclude(x => x.Customer)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.TargetInvoice!)
                    .ThenInclude(x => x.Customer)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.CustomerTransferVoucher!)
                    .ThenInclude(x => x.SourceCustomer)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.CustomerTransferVoucher!)
                    .ThenInclude(x => x.DestinationCustomer)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.CustomerTransferVoucher!)
                    .ThenInclude(x => x.SourceInvoice)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.CustomerTransferVoucher!)
                    .ThenInclude(x => x.DestinationInvoice)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.CheckPayment!)
                    .ThenInclude(x => x.Issuer)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.CheckPayment!)
                    .ThenInclude(x => x.IssuerFinancialAccount)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        var unpaid = item.TotalUnpaidAmount;
        var impact = unpaid * (item.InvoiceType == InvoiceType.Sell ? 1m : -1m); // Sell: + (بدهکار مثبت), Purchase: - (بستانکار منفی)

        // مانده قبلی (قبل از CreatedAt)
        var previousRemaining = await transactionRepository.GetCustomerRemainingListAsync(item.CustomerId,
            null,
            item.InvoiceDate.ToDateTime(TimeOnly.FromTimeSpan(item.CreatedAt.TimeOfDay)),
            cancellationToken: cancellationToken);

        decimal? exchangeRate = null;

        if (previousRemaining.Any())
        {
            var customerPreviousInvoices = await invoiceRepository.Get(new InvoicesByCustomerIdSpecification(item.CustomerId))
                .Where(x => x.CreatedAt < item.CreatedAt)
                .ToListAsync(cancellationToken);

            // Get the exchange rate from the previous invoices
            exchangeRate = customerPreviousInvoices
                .Select(x => x.UnpaidAmountExchangeRate)
                .FirstOrDefault(x => x.HasValue);
        }

        // مانده پس از فاکتور (previous + impact)
        var afterRemaining = new Dictionary<PriceUnit, decimal>(previousRemaining);
        afterRemaining[item.PriceUnit!] = afterRemaining.GetValueOrDefault(item.PriceUnit!, 0m) + impact; // مستقیم set

        var previousFormatted = FormatRemaining(previousRemaining, item.PriceUnit!, item.UnpaidPriceUnit, exchangeRate);
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
