using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Validators.Invoices;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.InvoicePayments;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class InvoiceService(
    IInvoiceRepository invoiceRepository,
    IInvoicePaymentRepository paymentRepository,
    IServerProductService productService,
    IServerReminderService reminderService,
    IPriceUnitRepository priceUnitRepositoy,
    IAccountingTransactionService transactionService,
    IServerInventoryStockService inventoryStockService,
    IMapper mapper,
    ILogger<InvoiceService> logger,
    InvoiceRequestDtoValidator validator,
    DeleteInvoiceValidator deleteValidator) : IInvoiceService
{
    public async Task CreateAsync(InvoiceRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await invoiceRepository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                await validator.ValidateAndThrowAsync(request, cancellationToken);

                #region Invoice (Create new invoice instance)    

                var basePriceUnit = await priceUnitRepositoy
                    .Get(new PriceUnitsSetAsDefaultSpecification())
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException("Base price unit not found");

                var invoice = Invoice.Create(request.InvoiceNumber,
                    request.UnpaidAmountExchangeRate,
                    request.ExchangeRate,
                    request.InvoiceType,
                    new CustomerId(request.CustomerId),
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

                #endregion

                #region InvoiceItems

                foreach (var coinItemDto in request.InvoiceCoinItems)
                {
                    invoice.AddCoinItem(coinItemDto.Id.HasValue
                            ? new InvoiceCoinItemId(coinItemDto.Id.Value)
                            : null,
                        new CoinId(coinItemDto.CoinId),
                        coinItemDto.UnitPrice,
                        coinItemDto.Quantity,
                        coinItemDto.ProfitPercent);
                }

                foreach (var currencyItemDto in request.InvoiceCurrencyItems)
                {
                    invoice.AddCurrencyItem(currencyItemDto.Id.HasValue
                            ? new InvoiceCurrencyItemId(currencyItemDto.Id.Value)
                            : null,
                        new PriceUnitId(currencyItemDto.CurrencyId),
                        currencyItemDto.UnitPrice,
                        currencyItemDto.Amount,
                        currencyItemDto.TaxPercent,
                        currencyItemDto.ProfitPercent);
                }

                await productService.SyncUsedProductsForInvoiceAsync(invoice, request.InvoiceUsedProducts, cancellationToken);

                await productService.SyncProductItemsAsync(invoice, request.InvoiceProductItems, cancellationToken);

                #endregion

                await invoiceRepository.CreateAsync(invoice, cancellationToken);

                #region InvoicePayments (Create invoice payments)

                var paymentsToCreate = request.InvoicePayments
                    .Select(dto => InvoicePayment.Create(
                        dto.PaymentDate,
                        dto.Amount,
                        dto.ExchangeRate,
                        invoice.Id,
                        new PriceUnitId(dto.PriceUnitId),
                        dto.FinancialAccountId.HasValue ? new FinancialAccountId(dto.FinancialAccountId.Value) : null,
                        dto.VoucherId.HasValue ? new PaymentVoucherId(dto.VoucherId.Value) : null,
                        dto.ReferenceNumber,
                        dto.Note))
                    .ToList();

                if (paymentsToCreate.Any())
                    await paymentRepository.CreateRangeAsync(paymentsToCreate, cancellationToken);

                #endregion

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
        {
            try
            {
                await validator.ValidateAndThrowAsync(request, cancellationToken);

                var invoice = await invoiceRepository
                    .Get(new InvoicesByIdSpecification(new InvoiceId(id)))
                    .Include(x => x.InvoicePayments)
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

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

                await transactionService.ClearTransactionsForInvoiceAsync(invoice, cancellationToken);
                await inventoryStockService.RemoveInventoryByInvoiceIdAsync(invoice.Id, null, cancellationToken);

                #region InvoiceItems

                invoice.ClearCoinItems();
                foreach (var coinItemDto in request.InvoiceCoinItems)
                {
                    invoice.AddCoinItem(coinItemDto.Id.HasValue
                            ? new InvoiceCoinItemId(coinItemDto.Id.Value)
                            : null,
                        new CoinId(coinItemDto.CoinId),
                        coinItemDto.UnitPrice,
                        coinItemDto.Quantity,
                        coinItemDto.ProfitPercent);
                }

                invoice.ClearCurrencyItems();
                foreach (var currencyItemDto in request.InvoiceCurrencyItems)
                {
                    invoice.AddCurrencyItem(currencyItemDto.Id.HasValue
                            ? new InvoiceCurrencyItemId(currencyItemDto.Id.Value)
                            : null,
                        new PriceUnitId(currencyItemDto.CurrencyId),
                        currencyItemDto.UnitPrice,
                        currencyItemDto.Amount,
                        currencyItemDto.TaxPercent,
                        currencyItemDto.ProfitPercent);
                }

                await productService.SyncUsedProductsForInvoiceAsync(invoice, request.InvoiceUsedProducts, cancellationToken);

                await productService.SyncProductItemsAsync(invoice, request.InvoiceProductItems, cancellationToken);

                #endregion

                await invoiceRepository.UpdateAsync(invoice, cancellationToken);

                #region InvoicePayments (Update invoice payments)

                var existingPayments = await paymentRepository
                    .Get(new InvoicePaymentsByInvoiceIdSpecification(invoice.Id))
                    .ToListAsync(cancellationToken);

                await paymentRepository.DeleteRangeAsync(existingPayments, cancellationToken);

                var paymentsToCreate = request.InvoicePayments
                    .Select(dto => InvoicePayment.Create(
                        dto.PaymentDate,
                        dto.Amount,
                        dto.ExchangeRate,
                        invoice.Id,
                        new PriceUnitId(dto.PriceUnitId),
                        dto.FinancialAccountId.HasValue ? new FinancialAccountId(dto.FinancialAccountId.Value) : null,
                        dto.VoucherId.HasValue ? new PaymentVoucherId(dto.VoucherId.Value) : null,
                        dto.ReferenceNumber,
                        dto.Note))
                    .ToList();

                if (paymentsToCreate.Any())
                    await paymentRepository.CreateRangeAsync(paymentsToCreate, cancellationToken);

                #endregion

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

    public async Task<GetInvoiceResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await invoiceRepository
            .Get(new InvoicesByIdSpecification(new InvoiceId(id)))
            .AsNoTracking()
            .Include(x => x.Customer!)
                .ThenInclude(x => x.CreditLimitPriceUnit)
            .Include(x => x.ProductItems)
                .ThenInclude(x => x.Product)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.SourceFinancialAccount!)
            .Include(x => x.CoinItems)
                .ThenInclude(x => x.Coin)
            .Include(x => x.CurrencyItems)
                .ThenInclude(x => x.Currency)
            .Include(x => x.UsedProducts)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetInvoiceResponse>(item);
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
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.SourceFinancialAccount!)
            .Include(x => x.CoinItems)
                .ThenInclude(x => x.Coin)
            .Include(x => x.CurrencyItems)
                .ThenInclude(x => x.Currency)
            .Include(x => x.UsedProducts)
                .ThenInclude(x => x.Product)
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
}