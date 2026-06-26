using FluentValidation;
using FluentValidation.Results;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CoinInstanceAggregate;
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
}