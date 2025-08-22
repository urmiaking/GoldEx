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
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.InvoicePayments;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.Constants;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using FluentValidation.Results;
using GoldEx.Sdk.Common.Extensions;
using Product = GoldEx.Server.Domain.ProductAggregate.Product;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class InvoiceService(
    IInvoiceRepository invoiceRepository,
    IInvoicePaymentRepository paymentRepository,
    ILedgerAccountRepository ledgerAccountRepository,
    IProductRepository productRepository,
    ICustomerService customerService,
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

                #region Customer (Create or update customer)

                Guid customerId;

                if (request.Customer.Id.HasValue)
                {
                    await customerService.UpdateAsync(request.Customer.Id.Value, request.Customer, cancellationToken);
                    customerId = request.Customer.Id.Value;
                }
                else
                {
                    customerId = await customerService.CreateAsync(request.Customer, cancellationToken);
                }

                #endregion

                #region LedgerAccount (Create ledger account if not exists)

                var parentAccountTitle = request.InvoiceType == InvoiceType.Sell
                    ? SystemLedgerAccounts.AccountsReceivable
                    : SystemLedgerAccounts.AccountsPayable;

                var parentLedgerAccount = await ledgerAccountRepository
                                              .Get(new LedgerAccountsByTitleSpecification(parentAccountTitle))
                                              .FirstOrDefaultAsync(cancellationToken)
                                          ?? throw new InvalidOperationException($"System ledger account '{parentAccountTitle}' not found.");

                var existingLedgerAccount = await ledgerAccountRepository
                    .Get(new LedgerAccountsByCustomerAndParentSpecification(new CustomerId(customerId), parentLedgerAccount.Id))
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingLedgerAccount is null)
                {
                    var customer = await customerService.GetAsync(customerId, cancellationToken)
                                   ?? throw new NotFoundException("Customer not found after creation.");

                    var ledgerAccountTitle = $"{parentLedgerAccount.Title} - {customer.FullName}";
                    var newLedgerAccount = LedgerAccount.CreateCustomerAccount(
                        ledgerAccountTitle,
                        new CustomerId(customer.Id),
                        parentLedgerAccount.AccountType,
                        parentLedgerAccount.Id);

                    await ledgerAccountRepository.CreateAsync(newLedgerAccount, cancellationToken);
                }

                #endregion

                #region Invoice (Create new invoice instance and populating owned entities)    

                var invoice = Invoice.Create(request.InvoiceNumber,
                    request.UnpaidAmountExchangeRate,
                    request.ExchangeRate,
                    request.InvoiceType,
                    new CustomerId(customerId),
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

                invoice.SetCoinItems(request.InvoiceCoinItems.Select(x =>
                    InvoiceCoinItem.Create(new CoinId(x.CoinId), x.UnitPrice, x.Quantity, x.ProfitPercent)));

                invoice.SetCurrencyItems(request.InvoiceCurrencyItems.Select(x =>
                    InvoiceCurrencyItem.Create(new PriceUnitId(x.CurrencyId), x.UnitPrice, x.Amount, x.TaxPercent, x.ProfitPercent)));

                foreach (var usedGold in request.InvoiceUsedProducts)
                {
                    ProductId? productId = null;
                    if (usedGold.IsSellable)
                    {
                        var product = Product.Create(usedGold.Description,
                            StringExtensions.GenerateRandomBarcode(),
                            usedGold.Weight,
                            0,
                            usedGold.ProductType,
                            usedGold.Fineness,
                            usedGold.GoldUnitType,
                            null,
                            null,
                            null);

                        await productRepository.CreateAsync(product, cancellationToken);
                        productId = product.Id;
                    }

                    invoice.AddUsedProduct(usedGold.Description, usedGold.Weight, usedGold.GramPrice,
                        usedGold.ExtraCostsAmount, usedGold.Fineness, usedGold.IsSellable, productId);
                }

                #endregion

                #region Product (Create or update products)

                foreach (var itemDto in request.InvoiceProductItems)
                {
                    Product? product;

                    if (!itemDto.Product.Id.HasValue)
                    {
                        var newProduct = Product.Create(itemDto.Product.Name,
                            itemDto.Product.Barcode,
                            itemDto.Product.Weight,
                            itemDto.Product.Wage,
                            itemDto.Product.ProductType,
                            itemDto.Product.Fineness,
                            itemDto.Product.GoldUnitType,
                            itemDto.Product.WageType,
                            itemDto.Product.WagePriceUnitId.HasValue
                                ? new PriceUnitId(itemDto.Product.WagePriceUnitId.Value)
                                : null,
                            itemDto.Product.ProductCategoryId.HasValue
                                ? new ProductCategoryId(itemDto.Product.ProductCategoryId.Value)
                                : null);

                        product = newProduct;

                        await productRepository.CreateAsync(newProduct, cancellationToken);

                        if (request.InvoiceType is InvoiceType.Sell)
                        {
                            if (!itemDto.CostPrice.HasValue)
                            {
                                throw new ValidationException("خطای اعتبارسنجی",
                                    new List<ValidationFailure>
                                    {
                                        new(nameof(itemDto.CostPrice),
                                            "وارد کردن نرخ خرید الزامی است")
                                    });
                            }
                        }
                    }
                    else
                    {
                        var existingProduct = await productRepository
                            .Get(new ProductsByIdSpecification(new ProductId(itemDto.Product.Id.Value)))
                            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                        existingProduct.SetName(itemDto.Product.Name);
                        existingProduct.SetBarcode(itemDto.Product.Barcode);
                        existingProduct.SetWeight(itemDto.Product.Weight);
                        existingProduct.SetFineness(itemDto.Product.Fineness);
                        existingProduct.SetGoldUnitType(itemDto.Product.GoldUnitType);
                        existingProduct.SetProductType(itemDto.Product.ProductType);
                        existingProduct.SetWage(itemDto.Product.Wage);
                        existingProduct.SetWageType(itemDto.Product.WageType);
                        existingProduct.SetProductCategory(
                            itemDto.Product.ProductCategoryId.HasValue
                                ? new ProductCategoryId(itemDto.Product.ProductCategoryId.Value)
                                : null);
                        existingProduct.SetWagePriceUnitId(
                            itemDto.Product.WagePriceUnitId.HasValue
                                ? new PriceUnitId(itemDto.Product.WagePriceUnitId.Value)
                                : null);
                        if (itemDto.Product.ProductType == ProductType.Jewelry)
                        {
                            existingProduct.SetGemStones(itemDto.Product.GemStones?.Select(s => GemStone.Create(s.Code,
                                s.Type,
                                s.Color,
                                s.Cut,
                                s.Carat,
                                s.Purity,
                                existingProduct.Id)));
                        }
                        else
                            existingProduct.ClearGemStones();

                        product = existingProduct;

                        await productRepository.UpdateAsync(existingProduct, cancellationToken);
                    }

                    invoice.AddProductItem(InvoiceProductItem.Create(itemDto.GramPrice,
                            itemDto.ProfitPercent,
                            itemDto.TaxPercent,
                            product.Id,
                            itemDto.CostPrice,
                            itemDto.CostPriceExchangeRate,
                            itemDto.CostPriceUnitId.HasValue ? new PriceUnitId(itemDto.CostPriceUnitId.Value) : null,
                            itemDto.IsInstantProduct)
                        .SetInvoice(invoice)
                        .RecalculateAmounts(product));
                }

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

                #region Customer (Create or update customer)

                Guid customerId;

                if (request.Customer.Id.HasValue)
                {
                    await customerService.UpdateAsync(request.Customer.Id.Value, request.Customer, cancellationToken);
                    customerId = request.Customer.Id.Value;
                }
                else
                {
                    customerId = await customerService.CreateAsync(request.Customer, cancellationToken);
                }

                #endregion

                #region LedgerAccount (Create ledger account if not exists)

                var parentAccountTitle = request.InvoiceType == InvoiceType.Sell
                    ? SystemLedgerAccounts.AccountsReceivable
                    : SystemLedgerAccounts.AccountsPayable;

                var parentLedgerAccount = await ledgerAccountRepository
                                              .Get(new LedgerAccountsByTitleSpecification(parentAccountTitle))
                                              .FirstOrDefaultAsync(cancellationToken)
                                          ?? throw new InvalidOperationException($"System ledger account '{parentAccountTitle}' not found.");

                var existingLedgerAccount = await ledgerAccountRepository
                    .Get(new LedgerAccountsByCustomerAndParentSpecification(new CustomerId(customerId), parentLedgerAccount.Id))
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingLedgerAccount is null)
                {
                    var customer = await customerService.GetAsync(customerId, cancellationToken)
                                   ?? throw new NotFoundException("Customer not found after creation.");

                    var ledgerAccountTitle = $"{parentLedgerAccount.Title} - {customer.FullName}";
                    var newLedgerAccount = LedgerAccount.CreateCustomerAccount(
                        ledgerAccountTitle,
                        new CustomerId(customer.Id),
                        parentLedgerAccount.AccountType,
                        parentLedgerAccount.Id);

                    await ledgerAccountRepository.CreateAsync(newLedgerAccount, cancellationToken);
                }

                #endregion

                #region Invoice (Update existing invoice)

                var invoice = await invoiceRepository
                    .Get(new InvoicesByIdSpecification(new InvoiceId(id)))
                    .Include(x => x.InvoicePayments)
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                invoice.SetPriceUnitId(new PriceUnitId(request.PriceUnitId));
                invoice.SetCustomerId(new CustomerId(customerId));
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

                invoice.SetCoinItems(request.InvoiceCoinItems.Select(x =>
                    InvoiceCoinItem.Create(new CoinId(x.CoinId), x.UnitPrice, x.Quantity, x.ProfitPercent)));

                invoice.SetCurrencyItems(request.InvoiceCurrencyItems.Select(x =>
                    InvoiceCurrencyItem.Create(new PriceUnitId(x.CurrencyId), x.UnitPrice, x.Amount, x.TaxPercent, x.ProfitPercent)));

                await transactionService.ClearTransactionsForInvoiceAsync(invoice, cancellationToken);
                await inventoryStockService.RemoveInventoryByInvoiceIdAsync(invoice.Id, null, cancellationToken);

                #endregion

                #region Product (Create or update products)

                invoice.ClearProductItems();

                foreach (var itemDto in request.InvoiceProductItems)
                {
                    Product product;

                    if (!itemDto.Product.Id.HasValue)
                    {
                        var newProduct = Product.Create(itemDto.Product.Name,
                            itemDto.Product.Barcode,
                            itemDto.Product.Weight,
                            itemDto.Product.Wage,
                            itemDto.Product.ProductType,
                            itemDto.Product.Fineness,
                            itemDto.Product.GoldUnitType,
                            itemDto.Product.WageType,
                            itemDto.Product.WagePriceUnitId.HasValue
                                ? new PriceUnitId(itemDto.Product.WagePriceUnitId.Value)
                                : null,
                            itemDto.Product.ProductCategoryId.HasValue
                                ? new ProductCategoryId(itemDto.Product.ProductCategoryId.Value)
                                : null);

                        product = newProduct;

                        await productRepository.CreateAsync(newProduct, cancellationToken);

                        if (request.InvoiceType is InvoiceType.Sell)
                        {
                            if (!itemDto.CostPrice.HasValue)
                            {
                                throw new ValidationException("خطای اعتبارسنجی",
                                    new List<ValidationFailure>
                                    {
                                        new(nameof(itemDto.CostPrice),
                                            "وارد کردن نرخ خرید الزامی است")
                                    });
                            }
                        }
                    }
                    else
                    {
                        var existingProduct = await productRepository
                            .Get(new ProductsByIdSpecification(new ProductId(itemDto.Product.Id.Value)))
                            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                        existingProduct.SetName(itemDto.Product.Name);
                        existingProduct.SetBarcode(itemDto.Product.Barcode);
                        existingProduct.SetWeight(itemDto.Product.Weight);
                        existingProduct.SetFineness(itemDto.Product.Fineness);
                        existingProduct.SetGoldUnitType(itemDto.Product.GoldUnitType);
                        existingProduct.SetProductType(itemDto.Product.ProductType);
                        existingProduct.SetWage(itemDto.Product.Wage);
                        existingProduct.SetWageType(itemDto.Product.WageType);
                        existingProduct.SetProductCategory(
                            itemDto.Product.ProductCategoryId.HasValue
                                ? new ProductCategoryId(itemDto.Product.ProductCategoryId.Value)
                                : null);
                        existingProduct.SetWagePriceUnitId(
                            itemDto.Product.WagePriceUnitId.HasValue
                                ? new PriceUnitId(itemDto.Product.WagePriceUnitId.Value)
                                : null);
                        if (itemDto.Product.ProductType == ProductType.Jewelry)
                        {
                            existingProduct.SetGemStones(itemDto.Product.GemStones?.Select(s => GemStone.Create(s.Code,
                                s.Type,
                                s.Color,
                                s.Cut,
                                s.Carat,
                                s.Purity,
                                existingProduct.Id)));
                        }
                        else
                            existingProduct.ClearGemStones();

                        product = existingProduct;

                        await productRepository.UpdateAsync(existingProduct, cancellationToken);
                    }

                    invoice.AddProductItem(InvoiceProductItem.Create(itemDto.GramPrice,
                            itemDto.ProfitPercent,
                            itemDto.TaxPercent,
                            product.Id,
                            itemDto.CostPrice,
                            itemDto.CostPriceExchangeRate,
                            itemDto.CostPriceUnitId.HasValue ? new PriceUnitId(itemDto.CostPriceUnitId.Value) : null,
                            itemDto.IsInstantProduct)
                        .SetInvoice(invoice)
                        .RecalculateAmounts(product));
                }

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
            .AsSplitQuery()
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
            .Include(x => x.PriceUnit)
            .Include(x => x.ProductItems)
                .ThenInclude(x => x.Product)
                    .ThenInclude(x => x!.ProductCategory)
            .Include(x => x.ProductItems)
                .ThenInclude(x => x.CostPriceUnit)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.PriceUnit)
            .Include(x => x.UnpaidPriceUnit)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.SourceFinancialAccount)
            .Include(x => x.CoinItems)
                .ThenInclude(x => x.Coin)
            .Include(x => x.CurrencyItems)
                .ThenInclude(x => x.Currency)
            .Include(x => x.Discounts)
                .ThenInclude(x => x.PriceUnit)
            .Include(x => x.ExtraCosts)
                .ThenInclude(x => x.PriceUnit)
            .AsSplitQuery()
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
                .ThenInclude(x => x.CreditLimitPriceUnit!)
            .Include(x => x.PriceUnit)
            .Include(x => x.ProductItems)
                .ThenInclude(x => x.Product)
                    .ThenInclude(x => x!.ProductCategory)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.PriceUnit)
            .Include(x => x.ProductItems)
                .ThenInclude(x => x.CostPriceUnit)
            .Include(x => x.UnpaidPriceUnit)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.SourceFinancialAccount)
            .Include(x => x.CoinItems)
                .ThenInclude(x => x.Coin)
            .Include(x => x.CurrencyItems)
                .ThenInclude(x => x.Currency)
            .Include(x => x.Discounts)
                .ThenInclude(x => x.PriceUnit)
            .Include(x => x.ExtraCosts)
                .ThenInclude(x => x.PriceUnit)
            .AsSplitQuery()
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
                .AsSplitQuery()
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
            if (productList.Count > 0)
                await productRepository.DeleteRangeAsync(productList, cancellationToken);

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
}