using FluentValidation;
using FluentValidation.Results;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Application.Validators.Reporting;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CoinInstanceAggregate;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Shared.Helpers;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.InventoryStocks;
using GoldEx.Server.Infrastructure.Specifications.InvoicePayments;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class ReportingService(
    ITransactionRepository transactionRepository,
    IInvoiceRepository invoiceRepository,
    IInvoicePaymentRepository paymentRepository,
    IInventoryStockRepository inventoryStockRepository,
    IMapper mapper) : IReportingService
{
    public async Task<ProfitAnalysisRpResponse> GetProfitAnalysisAsync(ProfitAnalysisRpRequest request,
        CancellationToken cancellationToken = default)
    {
        await new ProfitAnalysisRpRequestValidator()
            .ValidateAndThrowAsync(request, cancellationToken);
        var data = await invoiceRepository.GetProfitAnalysisDataAsync(request, cancellationToken);
        return ProfitAnalysisBuilder.Build(data.Lines, data.ProductsWithOtherMovements, request);
    }

    public async Task<List<LedgerAccountStatementRpResponse>> GetLedgerAccountStatementsAsync(
        LedgerAccountStatementRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var models = await transactionRepository.GetLedgerAccountStatementsAsync(request, cancellationToken);
        return mapper.Map<List<LedgerAccountStatementRpResponse>>(models);
    }

    public async Task<List<LedgerAccountTrialBalanceRpResponse>> GetLedgerAccountTrialBalanceAsync(LedgerAccountTrialBalanceRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var list = await transactionRepository.GetLedgerAccountTrialBalanceAsync(request, cancellationToken);
        return mapper.Map<List<LedgerAccountTrialBalanceRpResponse>>(list.Nodes);
    }

    public async Task<List<CustomerRemainingBalanceRpResponse>> GetCustomerRemainingBalanceAsync(CustomerRemainingBalanceRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var list = await transactionRepository.GetCustomerRemainingBalanceAsync(request, cancellationToken);
        return mapper.Map<List<CustomerRemainingBalanceRpResponse>>(list);
    }

    public async Task<List<CustomerTransactionRpResponse>> GetCustomerTransactionsAsync(CustomerTransactionRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var list = await transactionRepository.GetCustomerTransactionsAsync(request, cancellationToken);
        return mapper.Map<List<CustomerTransactionRpResponse>>(list);
    }

    /// <summary>
    /// Combines source invoices/payment documents with the authoritative customer ledger.
    /// Invoice and payment amounts are NOT added to ledger totals again.
    /// The full ledger is fetched up to ToDate before applying FromDate to keep opening balances correct.
    /// </summary>
    public async Task<CustomerComprehensiveRpResponse> GetCustomerComprehensiveAsync(
        CustomerComprehensiveRpRequest request, CancellationToken cancellationToken = default)
    {
        if (request.CustomerId == Guid.Empty)
            throw new ArgumentException("طرف حساب را انتخاب کنید.", nameof(request));
        if (request.FromDate.HasValue && request.ToDate.HasValue && request.FromDate > request.ToDate)
            throw new ArgumentException("بازه تاریخ نامعتبر است.", nameof(request));

        // EF tenant filters on Invoice and related entities are preserved.
        var invoices = await invoiceRepository
            .Get(new InvoicesByCustomerIdSpecification(new CustomerId(request.CustomerId)))
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.PriceUnit)
            .Include(x => x.ProductItems).ThenInclude(x => x.Product)
            .Include(x => x.UsedProducts).ThenInclude(x => x.Product)
            .Include(x => x.CoinItems).ThenInclude(x => x.CoinInstance).ThenInclude(x => x!.Coin)
            .Include(x => x.CurrencyItems).ThenInclude(x => x.Currency)
            .Include(x => x.Discounts)
            .Include(x => x.ExtraCosts)
            .Include(x => x.InvoicePayments!).ThenInclude(x => x.PriceUnit)
            .ToListAsync(cancellationToken);

        var from = request.FromDate.HasValue ? DateOnly.FromDateTime(request.FromDate.Value) : (DateOnly?)null;
        var to = request.ToDate.HasValue ? DateOnly.FromDateTime(request.ToDate.Value) : (DateOnly?)null;

        var invoiceRows = invoices
            .Where(x => (!from.HasValue || x.InvoiceDate >= from.Value) &&
                        (!to.HasValue || x.InvoiceDate <= to.Value))
            .OrderBy(x => x.InvoiceDate).ThenBy(x => x.InvoiceNumber)
            .Select(x => new CustomerComprehensiveInvoice(
                x.Id.Value, x.InvoiceNumber, x.InvoiceDate, x.InvoiceType, x.TradeScale,
                x.PriceUnit?.Title ?? "نامشخص",
                x.TotalAmountWithDiscountsAndExtraCosts, x.TotalDiscountAmount, x.TotalExtraCostAmount,
                request.IncludeDetails
                    ? x.ProductItems.Select(i => new CustomerComprehensiveItem(
                            "طلا و جواهر", i.Product?.Name ?? "کالا", i.Quantity, i.TotalWeight,
                            i.Product?.Fineness, i.ItemFinalAmount))
                        .Concat(x.UsedProducts.Select(i => new CustomerComprehensiveItem(
                            "طلای مستعمل", i.Description, i.Quantity, i.Weight,
                            i.Product?.Fineness, i.ItemFinalAmount)))
                        .Concat(x.CoinItems.Select(i => new CustomerComprehensiveItem(
                            "سکه", i.CoinInstance?.Coin?.Title ?? "سکه", i.Quantity,
                            null, null, i.ItemFinalAmount)))
                        .Concat(x.CurrencyItems.Select(i => new CustomerComprehensiveItem(
                            "ارز", i.Currency?.Title ?? "ارز", i.Amount,
                            null, null, i.ItemFinalAmount)))
                        .ToList()
                    : new List<CustomerComprehensiveItem>()))
            .ToList();

        var invoiceNumbers = invoices.ToDictionary(i => i.Id, i => i.InvoiceNumber);

        // Payments may belong to invoices outside the requested invoice-date range.
        var paymentRows = invoices.SelectMany(x => x.InvoicePayments ?? [])
            .Where(p => (!request.FromDate.HasValue || p.PaymentDate >= request.FromDate.Value) &&
                        (!request.ToDate.HasValue || p.PaymentDate <= request.ToDate.Value))
            .OrderBy(p => p.PaymentDate).ThenBy(p => p.Id.Value)
            .Select(p => new CustomerComprehensivePayment(
                p.Id.Value, p.InvoiceId.Value,
                invoiceNumbers[p.InvoiceId],
                p.PaymentDate, p.PaymentType, p.PaymentSide, p.Amount,
                p.PriceUnit?.Title ?? "نامشخص", p.ReferenceNumber, p.Note,
                p.PaymentVoucherId?.Value, p.CustomerTransferVoucherId?.Value))
            .ToList();

        var ledgerRequest = new CustomerTransactionRpRequest(
            request.CustomerId, null, null, null, request.ToDate);
        var ledgerModels = await transactionRepository.GetCustomerTransactionsAsync(ledgerRequest, cancellationToken);
        var completeLedger = mapper.Map<List<CustomerTransactionRpResponse>>(ledgerModels);

        var balances = completeLedger.GroupBy(t => t.PriceUnitTitle)
            .Select(g => new CustomerComprehensiveBalance(g.Key, g.Last().RunningBalance))
            .OrderBy(g => g.PriceUnitTitle).ToList();

        var visibleLedger = completeLedger
            .Where(t => !request.FromDate.HasValue || t.PostingDate >= request.FromDate.Value)
            .ToList();

        return new CustomerComprehensiveRpResponse(
            request.CustomerId, invoiceRows, paymentRows, visibleLedger, balances);
    }

    public async Task<List<SellInvoiceRpResponse>> GetSellInvoicesAsync(SellInvoiceRpRequest request, CancellationToken cancellationToken = default)
    {
        var list = await invoiceRepository
            .Get(new InvoicesReportSpecification(InvoiceType.Sell,
                request.PaymentStatus,
                request.PriceUnitId,
                request.CustomerId,
                request.FromDate,
                request.ToDate))
            .ToListAsync(cancellationToken);

        return mapper.Map<List<SellInvoiceRpResponse>>(list);
    }

    public async Task<List<PurchaseInvoiceRpResponse>> GetPurchaseInvoicesAsync(PurchaseInvoiceRpRequest request, CancellationToken cancellationToken = default)
    {
        var list = await invoiceRepository
            .Get(new InvoicesReportSpecification(InvoiceType.Purchase,
                request.PaymentStatus,
                request.PriceUnitId,
                request.CustomerId,
                request.FromDate,
                request.ToDate))
            .ToListAsync(cancellationToken);

        return mapper.Map<List<PurchaseInvoiceRpResponse>>(list);
    }

    public async Task<List<PaymentRpResponse>> GetPaymentsAsync(PaymentRpRequest request, CancellationToken cancellationToken = default)
    {
        var list = await paymentRepository
            .Get(new InvoicePaymentsByReportSpecification(request))
            .ToListAsync(cancellationToken);

        return mapper.Map<List<PaymentRpResponse>>(list);
    }

    public async Task<List<InvoicePaymentRpResponse>> GetInvoicePaymentsAsync(InvoicePaymentRpRequest request, CancellationToken cancellationToken = default)
    {
        var list = await paymentRepository
            .Get(new InvoicePaymentsByNumberSpecification(request.InvoiceNumber, request.InvoiceType))
            .ToListAsync(cancellationToken);

        return mapper.Map<List<InvoicePaymentRpResponse>>(list);
    }

    public async Task<List<InventoryKardexRpResponse>> GetInventoryKardexAsync(InventoryKardexRpRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.ProductId.HasValue && !request.CoinInstanceId.HasValue && !request.CurrencyId.HasValue)
            throw new ValidationException(new List<ValidationFailure> { new (nameof(request), "لطفا کالا را انتخاب کنید") });

        var list = await inventoryStockRepository
            .Get(new InventoryStocksKardexSpecification(request.ProductId.HasValue
                    ? new ProductId(request.ProductId.Value)
                    : null,
                request.CoinInstanceId.HasValue
                    ? new CoinInstanceId(request.CoinInstanceId.Value)
                    : null,
                request.CurrencyId.HasValue
                    ? new PriceUnitId(request.CurrencyId.Value)
                    : null))
            .Include(x => x.Invoice!.CurrencyItems)
                .ThenInclude(x => x.FinancialAccount)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<InventoryKardexRpResponse>>(list);
    }

    public async Task<List<ProductInventoryRpResponse>> GetProductInventoryAsync(ProductInventoryRpRequest request, CancellationToken cancellationToken = default)
    {
        var list = await inventoryStockRepository.GetProductsReportAsync(request, cancellationToken);
        return mapper.Map<List<ProductInventoryRpResponse>>(list);
    }

    public async Task<List<CoinInventoryRpResponse>> GetCoinInventoryAsync(CoinInventoryRpRequest request, CancellationToken cancellationToken = default)
    {
        var list = await inventoryStockRepository.GetCoinsReportAsync(request, cancellationToken);
        return mapper.Map<List<CoinInventoryRpResponse>>(list);
    }

    public async Task<List<CurrencyInventoryRpResponse>> GetCurrencyInventoryAsync(CurrencyInventoryRpRequest request, CancellationToken cancellationToken = default)
    {
        var list = await inventoryStockRepository.GetCurrenciesReportAsync(request, cancellationToken);
        return mapper.Map<List<CurrencyInventoryRpResponse>>(list);
    }

    public async Task<List<UsedGoldHiddenProfitRpResponse>> GetUsedGoldHiddenProfitAsync(
        UsedGoldHiddenProfitRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var list = await invoiceRepository
            .Get(new UsedGoldHiddenProfitSpecification(request.CustomerId, request.PriceUnitId, request.FromDate, request.ToDate))
            .Include(x => x.Customer)
            .Include(x => x.PriceUnit)
            .Include(x => x.UsedProducts)
                .ThenInclude(x => x.Product)
            .ToListAsync(cancellationToken);

        var response = new List<UsedGoldHiddenProfitRpResponse>();

        foreach (var invoice in list)
        {
            var exchangeRate = invoice.PriceUnit!.IsGoldBased ? null : invoice.ExchangeRate;

            foreach (var usedProduct in invoice.UsedProducts)
            {
                var fineness = usedProduct.Product != null ? usedProduct.Product.Fineness : 750m;

                // Convert actual weight to equivalent 750 weight
                var equivalentWeight = usedProduct.Weight * (fineness / 750m);

                // Calculate real value based on actual fineness (before deduction rate)
                var rate = exchangeRate ?? 1m;
                var realValue = equivalentWeight * usedProduct.GramPrice * rate * usedProduct.Quantity;

                // Hidden profit is the difference between real value and what we paid (ItemAmount)
                var hiddenProfit = realValue - usedProduct.ItemAmount;

                response.Add(new UsedGoldHiddenProfitRpResponse(
                    invoice.Id.Value,
                    invoice.InvoiceNumber,
                    invoice.InvoiceDate,
                    invoice.InvoiceType,
                    invoice.Customer?.FullName ?? string.Empty,
                    usedProduct.Description,
                    usedProduct.Weight,
                    fineness,
                    usedProduct.FinenessDeductionRate,
                    usedProduct.GramPrice,
                    usedProduct.Quantity,
                    invoice.ExchangeRate,
                    usedProduct.ItemAmount,
                    realValue,
                    hiddenProfit,
                    invoice.PriceUnit.Title));
            }
        }

        return response;
    }

    public async Task<List<CategorySalesRpResponse>> GetCategorySalesSummaryAsync(
        CategorySalesRpRequest request, CancellationToken cancellationToken = default)
    {
        return await invoiceRepository.GetCategorySalesSummaryAsync(request, cancellationToken);
    }

    public async Task<List<SoldProductItemRpResponse>> GetSoldProductItemsAsync(
        SoldProductItemRpRequest request, CancellationToken cancellationToken = default)
    {
        return await invoiceRepository.GetSoldProductItemsAsync(request, cancellationToken);
    }

    public async Task<List<CategorySalesComparisonRpResponse>> GetCategorySalesComparisonAsync(
        CategorySalesComparisonRpRequest request, CancellationToken cancellationToken = default)
    {
        return await invoiceRepository.GetCategorySalesComparisonAsync(request, cancellationToken);
    }
}
