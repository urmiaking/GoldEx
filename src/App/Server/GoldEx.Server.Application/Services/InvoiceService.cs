using FluentValidation;
using FluentValidation.Results;
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
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.InvoicePayments;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using Product = GoldEx.Server.Domain.ProductAggregate.Product;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class InvoiceService(
    IInvoiceRepository invoiceRepository,
    IInvoicePaymentRepository paymentRepository,
    IProductRepository productRepository,
    IBarcodeService barcodeService,
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

                var invoice = Invoice.Create(request.InvoiceNumber,
                    request.UnpaidAmountExchangeRate,
                    request.ExchangeRate,
                    request.InvoiceType,
                    new CustomerId(request.CustomerId),
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

                #region Coins

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

                #endregion

                #region CurrencyItems

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

                #endregion

                #region UsedGolds

                foreach (var usedGold in request.InvoiceUsedProducts)
                {
                    ProductId? productId = null;
                    if (usedGold.IsSellable)
                    {
                        var product = Product.Create(usedGold.Description,
                            usedGold.Weight,
                            0,
                            usedGold.ProductType,
                            usedGold.Fineness,
                            usedGold.UnitType,
                            null,
                            null,
                            null);

                        var barcode = await barcodeService.GenerateProductBarcodeAsync(product, cancellationToken);

                        product.SetBarcode(barcode);

                        await productRepository.CreateAsync(product, cancellationToken);
                        productId = product.Id;
                    }

                    invoice.AddUsedProduct(usedGold.Id.HasValue
                            ? new InvoiceUsedProductId(usedGold.Id.Value)
                            : null,
                        usedGold.Description,
                        usedGold.Weight,
                        usedGold.GramPrice,
                        usedGold.ExtraCostsAmount,
                        usedGold.Fineness,
                        usedGold.Quantity,
                        usedGold.IsSellable,
                        usedGold.ProductType,
                        usedGold.UnitType,
                        productId);
                }

                #endregion

                #region Product

                foreach (var itemDto in request.InvoiceProductItems)
                {
                    Product product;

                    // Step 1: Check if a product with the same attributes already exists
                    var productsWithSameName = await productRepository
                        .Get(new ProductsByNameSpecification(itemDto.Product.Name))
                        .ToListAsync(cancellationToken);

                    var identicalProduct = productsWithSameName.FirstOrDefault(x =>
                        x.WageType == itemDto.Product.WageType &&
                        x.Wage == itemDto.Product.Wage &&
                        x.Fineness == itemDto.Product.Fineness &&
                        x.ProductCategoryId == (itemDto.Product.ProductCategoryId.HasValue
                            ? new ProductCategoryId(itemDto.Product.ProductCategoryId.Value)
                            : null) &&
                        x.Weight == itemDto.Product.Weight);

                    if (identicalProduct != null)
                    {
                        // Found identical by attributes, use it
                        product = identicalProduct;
                    }
                    else
                    {
                        // Step 2: Actually create a new product
                        var newProduct = Product.Create(
                            itemDto.Product.Name,
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

                        if (string.IsNullOrEmpty(itemDto.Product.Barcode))
                        {
                            // Generate barcode and assign
                            var barcode = await barcodeService.GenerateProductBarcodeAsync(newProduct, cancellationToken);
                            newProduct.SetBarcode(barcode);
                        }
                        else
                        {
                            await barcodeService.ValidateBarcodeUniquenessAsync(itemDto.Product.Barcode, cancellationToken);
                            newProduct.SetBarcode(itemDto.Product.Barcode);
                        }

                        await productRepository.CreateAsync(newProduct, cancellationToken);
                        product = newProduct;
                    }

                    // Add product to invoice
                    invoice.AddProductItem(
                        itemDto.Id.HasValue ? new InvoiceProductItemId(itemDto.Id.Value) : null,
                        itemDto.GramPrice,
                        itemDto.ProfitPercent,
                        itemDto.TaxPercent,
                        itemDto.Quantity,
                        itemDto.CostPrice,
                        itemDto.CostPriceExchangeRate,
                        itemDto.CostPriceUnitId.HasValue ? new PriceUnitId(itemDto.CostPriceUnitId.Value) : null,
                        itemDto.IsInstantProduct,
                        product);
                }

                #endregion

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

                #region Invoice (Update existing invoice)

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

                #region Coins

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

                #endregion

                #region Currencies

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

                #endregion

                #region UsedProducts

                // STEP 1: BATCH DELETE
                var productIdsToDelete = invoice.UsedProducts
                    .Where(up => up.ProductId.HasValue)
                    .Select(up => up.ProductId!.Value)
                    .ToList();

                if (productIdsToDelete.Any())
                {
                    // One query to get them, one query to delete them
                    var productsToDelete = await productRepository
                        .Get(new ProductsByIdsSpecification(productIdsToDelete))
                        .ToListAsync(cancellationToken);

                    try
                    {
                        if (productsToDelete.Any())
                            await productRepository.DeleteRangeAsync(productsToDelete, cancellationToken);
                    }
                    catch (DbUpdateException e)
                    {
                        // This exception likely means that the product is referenced in other records (e.g., invoices).
                        logger.LogError(e, e.Message);
                        // A ui message in persian to inform the user to remove them manually before updating the invoice.
                        throw new ValidationException(new List<ValidationFailure>
                        {
                            new("UsedProducts",
                                "برخی از اجناس استفاده شده در این فاکتور در فاکتور دیگری استفاده شده‌اند و نمی‌توان آنها را ویرایش کرد. " +
                                "لطفاً ابتدا این اجناس را از فاکتورهای دیگر حذف کنید و سپس دوباره تلاش کنید.")
                        });
                    }
                }

                invoice.ClearUsedProducts();

                // STEP 2: BATCH CREATE (Same logic as in the CreateAsync optimization)
                // This logic can be extracted into a shared private method if desired.
                var newProductsToCreate = new List<Product>();
                var usedProductsWithNewProduct = new List<(InvoiceUsedProductDto Dto, Product NewProduct)>();

                foreach (var usedGold in request.InvoiceUsedProducts)
                {
                    if (usedGold.IsSellable)
                    {
                        var product = Product.Create(usedGold.Description,
                            usedGold.Weight,
                            0,
                            usedGold.ProductType,
                            usedGold.Fineness,
                            usedGold.UnitType,
                            null,
                            null,
                            null);

                        var barcode = await barcodeService.GenerateProductBarcodeAsync(product, cancellationToken);
                        product.SetBarcode(barcode);

                        newProductsToCreate.Add(product);
                        usedProductsWithNewProduct.Add((usedGold, product));
                    }
                    else
                    {
                        invoice.AddUsedProduct(usedGold.Id.HasValue
                                ? new InvoiceUsedProductId(usedGold.Id.Value)
                                : null,
                            usedGold.Description,
                            usedGold.Weight,
                            usedGold.GramPrice,
                            usedGold.ExtraCostsAmount,
                            usedGold.Fineness,
                            usedGold.Quantity,
                            false,
                            usedGold.ProductType,
                            usedGold.UnitType,
                            null);
                    }
                }

                if (newProductsToCreate.Any()) 
                    await productRepository.CreateRangeAsync(newProductsToCreate, cancellationToken);

                foreach (var (dto, newProduct) in usedProductsWithNewProduct) 
                    invoice.AddUsedProduct(dto.Id.HasValue
                            ? new InvoiceUsedProductId(dto.Id.Value)
                            : null,
                        dto.Description,
                        dto.Weight,
                        dto.GramPrice,
                        dto.ExtraCostsAmount,
                        dto.Fineness,
                        dto.Quantity,
                        true,
                        dto.ProductType,
                        dto.UnitType,
                        newProduct.Id);

                #endregion

                #endregion

                #region Products

                invoice.ClearProductItems();

                foreach (var itemDto in request.InvoiceProductItems)
                {
                    Product product;

                    // Step 1: Try to find an identical product in the DB
                    var identicalProduct = itemDto.Product.Id.HasValue
                        ? await productRepository
                            .Get(new ProductsByIdSpecification(new ProductId(itemDto.Product.Id.Value)))
                            .FirstOrDefaultAsync(cancellationToken)
                        : null;

                    if (identicalProduct != null)
                    {
                        // Found in DB, use it
                        product = identicalProduct;
                    }
                    else
                    {
                        // Step 2: Check if a product with the same attributes already exists
                        var productsWithSameName = await productRepository
                            .Get(new ProductsByNameSpecification(itemDto.Product.Name))
                            .ToListAsync(cancellationToken);

                        identicalProduct = productsWithSameName.FirstOrDefault(x =>
                            x.WageType == itemDto.Product.WageType &&
                            x.Wage == itemDto.Product.Wage &&
                            x.Fineness == itemDto.Product.Fineness &&
                            x.ProductCategoryId == (itemDto.Product.ProductCategoryId.HasValue
                                ? new ProductCategoryId(itemDto.Product.ProductCategoryId.Value)
                                : null) &&
                            x.Weight == itemDto.Product.Weight);

                        if (identicalProduct != null)
                        {
                            // Found identical by attributes, use it
                            product = identicalProduct;
                        }
                        else
                        {
                            // Step 3: Actually create a new product
                            var newProduct = Product.Create(
                                itemDto.Product.Name,
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

                            // Generate barcode and assign
                            var barcode = await barcodeService.GenerateProductBarcodeAsync(newProduct, cancellationToken);
                            newProduct = newProduct.SetBarcode(barcode);

                            await productRepository.CreateAsync(newProduct, cancellationToken);
                            product = newProduct;
                        }
                    }

                    // Add product to invoice
                    invoice.AddProductItem(
                        itemDto.Id.HasValue ? new InvoiceProductItemId(itemDto.Id.Value) : null,
                            itemDto.GramPrice,
                            itemDto.ProfitPercent,
                            itemDto.TaxPercent,
                            itemDto.Quantity,
                            itemDto.CostPrice,
                            itemDto.CostPriceExchangeRate,
                            itemDto.CostPriceUnitId.HasValue ? new PriceUnitId(itemDto.CostPriceUnitId.Value) : null,
                            itemDto.IsInstantProduct,
                        product);
                }

                #endregion

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
                .ThenInclude(x => x.SourceFinancialAccount!)
                    .ThenInclude(x => x.PriceUnit)
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