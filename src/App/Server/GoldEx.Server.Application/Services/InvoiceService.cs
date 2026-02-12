using DevExpress.XtraReports.Services;
using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Validators.Invoices;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Server.Infrastructure.Specifications.InvoicePayments;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class InvoiceService(
    IInvoiceRepository invoiceRepository,
    IInvoicePaymentRepository paymentRepository,
    ICustomerRepository customerRepository,
    IServerProductService productService,
    IServerReminderService reminderService,
    IPriceUnitRepository priceUnitRepository,
    IAccountingTransactionService transactionService,
    IServerInventoryStockService inventoryStockService,
    IServerInvoicePaymentService invoicePaymentService,
    IServerCoinInstanceService coinInstanceService,
    IFinancialAccountService financialAccountService,
    IReportProvider reportProvider,
    IMapper mapper,
    ILogger<InvoiceService> logger,
    InvoiceRequestDtoValidator validator,
    DeleteInvoiceValidator deleteValidator) : IInvoiceService, IServerInvoiceService
{
    public async Task CreateAsync(InvoiceRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await invoiceRepository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                await validator.ValidateAndThrowAsync(request, cancellationToken);

                #region Invoice (Create new invoice instance)    

                var basePriceUnit = await priceUnitRepository
                    .Get(new PriceUnitsSetAsDefaultSpecification())
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException("Base price unit not found");

                var targetPriceUnit = await priceUnitRepository
                    .Get(new PriceUnitsByIdSpecification(new PriceUnitId(request.PriceUnitId)))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Target price unit not found");

                var customer = await customerRepository
                    .Get(new CustomersByIdSpecification(new CustomerId(request.CustomerId)))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Customer not found");

                var invoice = Invoice.Create(request.InvoiceNumber,
                    request.UnpaidAmountExchangeRate,
                    request.ExchangeRate,
                    request.InvoiceType,
                    request.TradeScale,
                    customer.Id,
                    basePriceUnit.Id,
                    new PriceUnitId(request.PriceUnitId),
                    request.UnpaidPriceUnitId.HasValue ? new PriceUnitId(request.UnpaidPriceUnitId.Value) : null,
                    DateOnly.FromDateTime(request.InvoiceDate),
                    request.DueDate.HasValue
                        ? DateOnly.FromDateTime(request.DueDate.Value)
                        : null);

                invoice.SetExtraCosts(request.InvoiceExtraCosts.Select(x =>
                    InvoiceExtraCost.Create(x.Amount, x.ExchangeRate, new PriceUnitId(x.PriceUnitId), x.Description)));

                invoice.SetDiscounts(request.InvoiceDiscounts.Select(x =>
                    InvoiceDiscount.Create(x.Amount, x.ExchangeRate, new PriceUnitId(x.PriceUnitId), x.Description)));

                invoice.SetPriceUnit(targetPriceUnit);

                invoice.SetCustomer(customer);

                #endregion

                #region InvoiceItems

                foreach (var currencyItemDto in request.InvoiceCurrencyItems)
                {
                    invoice.AddCurrencyItem(currencyItemDto.Id.HasValue
                            ? new InvoiceCurrencyItemId(currencyItemDto.Id.Value)
                            : null,
                        new PriceUnitId(currencyItemDto.CurrencyId),
                        new FinancialAccountId(currencyItemDto.FinancialAccountId),
                        currencyItemDto.UnitPrice,
                        currencyItemDto.Amount,
                        currencyItemDto.TaxPercent,
                        currencyItemDto.ProfitPercent);
                }

                await coinInstanceService.SyncCoinItemsAsync(invoice, request.InvoiceCoinItems, cancellationToken);
                await productService.SyncUsedProductsForInvoiceAsync(invoice, request.InvoiceUsedProducts, cancellationToken);
                await productService.SyncProductItemsAsync(invoice, request.InvoiceProductItems, cancellationToken);

                #endregion

                await invoiceRepository.CreateAsync(invoice, cancellationToken);

                await invoicePaymentService.SyncPaymentsWithInvoiceAsync(invoice, request.InvoicePayments, cancellationToken);
                await inventoryStockService.CreateInvoiceInventoryAsync(invoice, cancellationToken);
                await transactionService.SetTransactionsForInvoiceAsync(invoice, cancellationToken);

                await dbTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    public async Task UpdateAsync(Guid id, InvoiceRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await invoiceRepository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        try
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var invoice = await invoiceRepository
                .Get(new InvoicesByIdSpecification(new InvoiceId(id)))
                .Include(x => x.InvoicePayments!)
                    .ThenInclude(x => x.LedgerAccount!.Customer)
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException();

            invoice.SetPriceUnitId(new PriceUnitId(request.PriceUnitId));
            invoice.SetCustomerId(new CustomerId(request.CustomerId));
            invoice.SetInvoiceDate(DateOnly.FromDateTime(request.InvoiceDate));
            invoice.SetDueDate(request.DueDate.HasValue ? DateOnly.FromDateTime(request.DueDate.Value) : null);
            invoice.SetInvoiceNumber(request.InvoiceNumber);
            invoice.SetExchangeRate(request.ExchangeRate);
            invoice.SetUnpaidAmountExchangeRate(request.UnpaidAmountExchangeRate);
            invoice.SetUnpaidPriceUnitId(request.UnpaidPriceUnitId.HasValue
                ? new PriceUnitId(request.UnpaidPriceUnitId.Value)
                : null);

            invoice.SetDiscounts(request.InvoiceDiscounts.Select(x =>
                InvoiceDiscount.Create(x.Amount, x.ExchangeRate, new PriceUnitId(x.PriceUnitId), x.Description)));

            invoice.SetExtraCosts(request.InvoiceExtraCosts.Select(x =>
                InvoiceExtraCost.Create(x.Amount, x.ExchangeRate, new PriceUnitId(x.PriceUnitId), x.Description)));

            #region InvoiceItems

            invoice.ClearCurrencyItems();
            foreach (var currencyItemDto in request.InvoiceCurrencyItems)
            {
                invoice.AddCurrencyItem(currencyItemDto.Id.HasValue
                        ? new InvoiceCurrencyItemId(currencyItemDto.Id.Value)
                        : null,
                    new PriceUnitId(currencyItemDto.CurrencyId),
                    new FinancialAccountId(currencyItemDto.FinancialAccountId),
                    currencyItemDto.UnitPrice,
                    currencyItemDto.Amount,
                    currencyItemDto.TaxPercent,
                    currencyItemDto.ProfitPercent);
            }

            await coinInstanceService.SyncCoinItemsAsync(invoice, request.InvoiceCoinItems, cancellationToken);
            await productService.SyncUsedProductsForInvoiceAsync(invoice, request.InvoiceUsedProducts, cancellationToken);
            await productService.SyncProductItemsAsync(invoice, request.InvoiceProductItems, cancellationToken);

            #endregion

            await invoiceRepository.UpdateAsync(invoice, cancellationToken);

            await invoicePaymentService.SyncPaymentsWithInvoiceAsync(invoice, request.InvoicePayments, cancellationToken);
            await transactionService.ReplaceTransactionsForInvoiceAsync(invoice, cancellationToken);
            await inventoryStockService.ReplaceInventoryForInvoiceAsync(invoice, cancellationToken);

            await dbTransaction.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            await dbTransaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<PagedList<GetInvoiceListResponse>> GetListAsync(RequestFilter filter, InvoiceFilter invoiceFilter,
        Guid? customerId, CancellationToken cancellationToken = default)
    {
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        var spec = new InvoicesByFilterSpecification(
            filter,
            invoiceFilter,
            customerId.HasValue ? new CustomerId(customerId.Value) : null
        );

        var data = await invoiceRepository
            .Get(spec)
            .AsNoTracking()
            .Include(x => x.ProductItems)
                .ThenInclude(x => x.Product)
            .Include(x => x.InvoicePayments)
            .ToListAsync(cancellationToken);

        var totalCount = await invoiceRepository.CountAsync(spec, cancellationToken);

        return new PagedList<GetInvoiceListResponse>
        {
            Data = mapper.Map<List<GetInvoiceListResponse>>(data),
            Skip = skip,
            Take = take,
            Total = totalCount
        };
    }

    public async Task<List<GetTinyInvoiceResponse>> GetCustomerInvoicesAsync(Guid customerId,
        RequestFilter filter, CancellationToken cancellationToken = default)
    {
        var invoices = await invoiceRepository
            .Get(new InvoicesByCustomerIdSpecification(new CustomerId(customerId), filter))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetTinyInvoiceResponse>>(invoices);
    }

    public async Task<GetInvoiceResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await invoiceRepository
            .Get(new InvoicesByIdSpecification(new InvoiceId(id)))
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
                .ThenInclude(x => x.TargetInvoice!.InvoicePayments!)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        var result = mapper.Map<GetInvoiceResponse>(item);

        var updatedPayments = new List<GetInvoicePaymentResponse>();
        foreach (var payment in result.InvoicePayments)
        {
            var priceUnitId = payment.PriceUnit.Id;
            var financialAccounts = await financialAccountService
                .GetTitlesAsync(null, priceUnitId, cancellationToken);

            updatedPayments.Add(payment with { FinancialAccounts = financialAccounts });
        }

        return result with { InvoicePayments = updatedPayments };
    }

    public async Task<GetInvoiceResponse> GetAsync(long invoiceNumber, InvoiceType invoiceType,
        CancellationToken cancellationToken = default)
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
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetInvoiceResponse>(item);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await invoiceRepository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        try
        {
            var item = await invoiceRepository
                .Get(new InvoicesByIdSpecification(new InvoiceId(id)))
                .Include(x => x.ProductItems)
                    .ThenInclude(x => x.Product)
                .Include(x => x.InvoicePayments)
                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

            await deleteValidator.ValidateAndThrowAsync(item, cancellationToken);
            await transactionService.ClearTransactionsForInvoiceAsync(item, cancellationToken);
            await inventoryStockService.RemoveInventoryByInvoiceIdAsync(item.Id, null, cancellationToken);

            var payments = await paymentRepository
                .Get(new InvoicePaymentsByInvoiceIdSpecification(item.Id))
                .ToListAsync(cancellationToken);

            if (payments.Count > 0)
                await paymentRepository.DeleteRangeAsync(payments, cancellationToken);

            await invoiceRepository.DeleteAsync(item, cancellationToken);

            var productsToDelete = item.InvoiceType == InvoiceType.Sell
                ? item.ProductItems.Where(x => x.IsInstantProduct).Select(x => x.Product!)
                : item.ProductItems.Select(x => x.Product!);

            var productList = productsToDelete.ToList();
            if (productList.Any())
                await productService.DeleteRangeAsync(productList, cancellationToken);

            await dbTransaction.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            await dbTransaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<GetInvoiceNumberResponse> GetLastNumberAsync(InvoiceType invoiceType,
        CancellationToken cancellationToken = default)
    {
        var number = await invoiceRepository.GetLastNumberAsync(invoiceType, cancellationToken);

        return new GetInvoiceNumberResponse(number);
    }

    public Task SendReminderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return reminderService.SendReminderAsync(id, cancellationToken);
    }

    public async Task<byte[]> GeneratePdfAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var reportId = await GenerateReportIdAsync(id, cancellationToken);

        var report = reportProvider.GetReport(reportId, null);
        using var ms = new MemoryStream();
        await report.ExportToPdfAsync(ms, token: cancellationToken);
        ms.Position = 0;
        return ms.ToArray();
    }

    private async Task<string> GenerateReportIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository
            .Get(new InvoicesByIdSpecification(new InvoiceId(id)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        var reportId = $"InvoiceReport?invoiceNumber={invoice.InvoiceNumber}&invoiceType={invoice.InvoiceType}";

        return reportId;
    }
}