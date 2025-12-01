using FluentValidation;
using FluentValidation.Results;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Validators.Products;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class ProductService(
    IProductRepository repository,
    IBarcodeGeneratorService barcodeGenerator,
    IServerBarcodeReservationService barcodeReservationService,
    ICustomerRepository customerRepository,
    IAccountingTransactionService transactionService,
    IServerInventoryStockService inventoryStockService,
    IMapper mapper,
    ILogger<ProductService> logger,
    ProductRequestDtoValidator validator,
    UpdateProductRequestValidator updateValidator,
    DeleteProductValidator deleteValidator) : IProductService, IServerProductService
{
    public async Task<List<GetProductResponse>> GetListAsync(string name, ProductType productType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
            return [];

        var products = await repository
            .Get(new ProductsByNameSpecification(name, productType))
            .AsNoTracking()
            .AsSplitQuery()
            .GroupBy(x => x.Name)
            .Take(8)
            .Select(x => x.First())
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetProductResponse>>(products);
    }

    public async Task<GetProductResponse?> GetAsync(string barcode, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new ProductsByBarcodeSpecification(barcode))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? null : mapper.Map<GetProductResponse>(item);
    }

    public Task<PagedList<GetProductResponse>> GetListAsync(RequestFilter filter, ProductFilter productFilter,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(Guid id, ProductRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                await validator.ValidateAndThrowAsync(request, cancellationToken);

                await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

                var item = await repository
                    .Get(new ProductsByIdSpecification(new ProductId(id)))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                var oldWeight = item.Weight;

                // Step 1. Update product properties using existing UpdateAsync method
                await UpdateAsync(item.Id, request, InvoiceType.Purchase, cancellationToken);

                // Step 2. Update inventory stocks and transactions if weight has changed
                if (oldWeight != request.Weight)
                {
                    var (outStock, inStock) = await inventoryStockService.UpdateStockAsync(item.Id, request.Weight, cancellationToken);
                    await transactionService.AddWeightChangeTransactionAsync(item.Id, oldWeight, request.Weight, outStock?.Id, inStock?.Id, cancellationToken);
                }

                await repository.UpdateAsync(item, cancellationToken);
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

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new ProductsByIdSpecification(new ProductId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        await deleteValidator.ValidateAndThrowAsync(item, cancellationToken);
        await repository.DeleteAsync(item, cancellationToken);
    }

    #region Server side services

    public async Task<Product> UpdateAsync(ProductId id, ProductRequestDto request, InvoiceType invoiceType, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var item = await repository
            .Get(new ProductsByIdSpecification(id))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        if (item.ProductType is ProductType.MoltenGold)
        {
            string? assayerName = null;

            if (request.MoltenGold?.AssayerId is Guid assayerId)
            {
                var assayer = await customerRepository
                                  .Get(new CustomersByIdSpecification(new CustomerId(assayerId)))
                                  .FirstOrDefaultAsync(cancellationToken)
                              ?? throw new NotFoundException();

                assayerName = assayer.FullName;
            }

            var assayNumber = request.MoltenGold?.AssayNumber;
            string moltenGoldName;

            if (!string.IsNullOrWhiteSpace(assayerName) && !string.IsNullOrWhiteSpace(assayNumber))
                moltenGoldName = $"آبشده ({assayNumber}) - {assayerName}";
            else if (!string.IsNullOrWhiteSpace(assayNumber))
                moltenGoldName = $"آبشده ({assayNumber})";
            else
                moltenGoldName = request.Name ?? item.Name;

            item.SetName(moltenGoldName);

            if (request.MoltenGold is not null)
            {
                item.SetMoltenGold(request.MoltenGold.AssayNumber,
                    request.MoltenGold.AssayDate,
                    request.MoltenGold.AssayerId.HasValue
                        ? new CustomerId(request.MoltenGold.AssayerId.Value)
                        : null);
            }
        }
        else
        {
            item.SetName(request.Name!);
        }

        if (invoiceType is InvoiceType.Purchase)
        {
            item.SetWage(request.Wage);
            item.SetWageType(request.WageType);
        }

        item.SetWeight(request.Weight); // TODO: check weight change impact on invoice
        item.SetProductType(request.ProductType);
        item.SetFineness(request.Fineness);
        item.SetGoldUnitType(request.GoldUnitType);

        if (request.ProductCategoryId.HasValue)
            item.SetProductCategory(new ProductCategoryId(request.ProductCategoryId.Value));
        else
            item.SetProductCategory(null);

        if (request.WagePriceUnitId.HasValue)
            item.SetWagePriceUnitId(new PriceUnitId(request.WagePriceUnitId.Value));
        else
            item.SetWagePriceUnitId(null);

        if (request.ProductType is ProductType.Jewelry)
        {
            item.SetGemStones(request.GemStones?.Select(s => GemStone.Create(StringExtensions.GenerateRandomCode(5),
                s.Type,
                s.Color,
                s.Cut,
                s.Carat,
                s.Purity,
                s.Cost,
                item.Id)));
        }
        else
            item.ClearGemStones();

        await repository.UpdateAsync(item, cancellationToken);

        return item;
    }

    public async Task<Product> CreateProductAsync(ProductRequestDto request, InvoiceId? invoiceId,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        Product product;

        if (request.ProductType is ProductType.MoltenGold)
        {
            string? assayerName = null;

            if (request.MoltenGold is { AssayerId: not null })
            {
                var assayer = await customerRepository
                    .Get(new CustomersByIdSpecification(new CustomerId(request.MoltenGold.AssayerId.Value)))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                assayerName = assayer.FullName;
            }

            var assayNumber = request.MoltenGold?.AssayNumber;

            string? moltenGoldName;

            if (!string.IsNullOrWhiteSpace(assayerName) && !string.IsNullOrWhiteSpace(assayNumber))
                moltenGoldName = $"آبشده ({assayNumber}) - {assayerName}";
            else if (!string.IsNullOrWhiteSpace(assayNumber))
                moltenGoldName = $"آبشده ({assayNumber})";
            else
                moltenGoldName = request.Name ?? $"طلای آبشده عیار {request.Fineness:G29}";

            product = Product.CreateMoltenGold(moltenGoldName,
                request.Weight,
                request.Wage,
                request.Fineness,
                request.GoldUnitType,
                request.WageType,
                request.WagePriceUnitId.HasValue
                    ? new PriceUnitId(request.WagePriceUnitId.Value)
                    : null,
                request.ProductCategoryId.HasValue
                    ? new ProductCategoryId(request.ProductCategoryId.Value)
                    : null,
                request.MoltenGold != null
                    ? MoltenGold.Create(request.MoltenGold.AssayNumber,
                        request.MoltenGold.AssayDate,
                        request.MoltenGold.AssayerId.HasValue
                            ? new CustomerId(request.MoltenGold.AssayerId.Value)
                            : null)
                    : null);
        }
        else
        {
            var item = Product.Create(
            request.Name!,
            request.Weight,
            request.Wage,
            request.ProductType,
            request.Fineness,
            request.GoldUnitType,
            request.WageType,
            request.WagePriceUnitId.HasValue ? new PriceUnitId(request.WagePriceUnitId.Value) : null,
            request.StonePriceUnitId.HasValue ? new PriceUnitId(request.StonePriceUnitId.Value) : null,
            request.ProductCategoryId.HasValue ? new ProductCategoryId(request.ProductCategoryId.Value) : null);

            if (request.ProductType is ProductType.Jewelry)
            {
                item.SetGemStones(request.GemStones?.Select(s => GemStone.Create(StringExtensions.GenerateRandomCode(5),
                    s.Type,
                    s.Color,
                    s.Cut,
                    s.Carat,
                    s.Purity,
                    s.Cost,
                    item.Id)));
            }

            product = item;
        }

        if (string.IsNullOrEmpty(request.Barcode))
        {
            // Generate directly (standalone product creation scenario)
            var categoryId = request.ProductCategoryId.HasValue ? new ProductCategoryId(request.ProductCategoryId.Value) : (ProductCategoryId?)null;
            var barcode = await barcodeGenerator.GenerateNextAsync(request.ProductType, categoryId, cancellationToken);
            product.SetBarcode(barcode);
        }
        else
        {
            // Attempt to commit reservation. If not found, validate uniqueness.
            await barcodeReservationService.CommitAsync(request.Barcode, invoiceId?.Value, cancellationToken);
            await barcodeGenerator.ValidateUniquenessAsync(request.Barcode, cancellationToken);
            product.SetBarcode(request.Barcode);
        }

        await repository.CreateAsync(product, cancellationToken);

        return product;
    }

    public async Task SyncUsedProductsForInvoiceAsync(Invoice invoice, IEnumerable<InvoiceUsedProductDto> usedProductDtos, CancellationToken cancellationToken = default)
    {
        // --- بخش ۱: حذف محصولات قدیمی ---

        // ابتدا ID محصولاتی که به آیتم‌های طلای کارکرده قدیمی متصل بودند را پیدا می‌کنیم
        var productIdsToDelete = invoice.UsedProducts
            .Where(up => up.ProductId.HasValue)
            .Select(up => up.ProductId!.Value)
            .ToList();

        if (productIdsToDelete.Any())
        {
            var productsToDelete = await repository
                .Get(new ProductsByIdsSpecification(productIdsToDelete))
                .ToListAsync(cancellationToken);

            // تلاش برای حذف دسته‌ای محصولات
            if (productsToDelete.Any())
            {
                try
                {
                    await repository.DeleteRangeAsync(productsToDelete, cancellationToken);
                }
                catch (DbUpdateException e)
                {
                    logger.LogError(e, "Failed to delete used products linked to other invoices.");
                    throw new ValidationException(new List<ValidationFailure>
                    {
                        new("UsedProducts",
                        "برخی از اجناس استفاده شده در این فاکتور در فاکتور دیگری استفاده شده‌اند و نمی‌توان آنها را ویرایش کرد. " +
                        "لطفاً ابتدا این اجناس را از فاکتورهای دیگر حذف کنید و سپس دوباره تلاش کنید.")
                    });
                }
            }
        }

        invoice.ClearUsedProducts();

        // --- بخش ۲: ایجاد محصولات جدید و افزودن به فاکتور ---
        var usedProductsWithNewProduct = new List<(InvoiceUsedProductDto Dto, Product NewProduct)>();

        foreach (var dto in usedProductDtos)
        {
            if (!dto.IsBroken)
            {
                var product = Product.Create(dto.Description,
                    dto.Weight,
                    0,
                    ProductType.Gold,
                    dto.Fineness,
                    dto.UnitType,
                    null,
                    null,
                    null,
                    null);

                var barcode = await barcodeGenerator.GenerateNextAsync(ProductType.Gold, null, cancellationToken);
                product.SetBarcode(barcode);

                await repository.CreateAsync(product, cancellationToken);

                usedProductsWithNewProduct.Add((dto, product));
            }
            else
            {
                var product = Product.CreateBrokenProduct(dto.Description, dto.Weight, dto.Fineness, dto.UnitType);

                var barcode = await barcodeGenerator.GenerateNextAsync(dto.ProductType, null, cancellationToken);
                product.SetBarcode(barcode);

                await repository.CreateAsync(product, cancellationToken);

                usedProductsWithNewProduct.Add((dto, product));
            }
        }

        // افزودن آیتم‌های طلای کارکرده (که محصول جدید برایشان ساخته شده) به فاکتور
        foreach (var (dto, newProduct) in usedProductsWithNewProduct)
        {
            invoice.AddUsedProduct(dto.Id.HasValue
                            ? new InvoiceUsedProductId(dto.Id.Value)
                            : null,
                        dto.Description,
                        dto.Weight,
                        dto.GramPrice,
                        dto.ExtraCostsAmount,
                        dto.Fineness,
                        dto.FinenessDeductionRate,
                        dto.Quantity,
                        dto.IsBroken,
                        dto.ProductType,
                        dto.UnitType,
                        newProduct.Id);
        }
    }

    public async Task SyncProductItemsAsync(Invoice invoice, IEnumerable<InvoiceProductItemDto> requestedItems, CancellationToken cancellationToken = default)
    {
        var existingItems = invoice.ProductItems.ToList();
        var requestedDtos = requestedItems.ToList();

        // 1- Remove products that are not in the request
        var itemsToDelete = existingItems
            .Where(e => requestedDtos.All(r => r.Id != e.Id.Value))
            .ToList();

        foreach (var itemToDelete in itemsToDelete)
        {
            invoice.RemoveProductItem(itemToDelete);

            // اگر این آیتم مربوط به فاکتور خرید بود و یا فاکتور فروشی که محصول به صورت آنی ایجاد شده بود
            if (invoice.InvoiceType == InvoiceType.Purchase || itemToDelete.IsInstantProduct)
            {
                // قبل از حذف آیتم، باید محصول مرتبط با آن را حذف کنیم
                await DeleteAsync(itemToDelete.ProductId.Value, cancellationToken);
            }
        }

        // --- ۲. پردازش آیتم‌های موجود در درخواست ---
        foreach (var itemDto in requestedDtos)
        {
            // update product item
            if (itemDto.Id.HasValue)
            {
                var existingItem = existingItems.FirstOrDefault(e => e.Id.Value == itemDto.Id.Value);
                if (existingItem != null)
                {
                    // Update product
                    var product = await UpdateAsync(existingItem.ProductId, itemDto.Product, invoice.InvoiceType, cancellationToken);

                    if (invoice.InvoiceType is InvoiceType.Sell)
                    {
                        existingItem.UpdateSaleItem(itemDto.GramPrice,
                            itemDto.ProfitPercent,
                            itemDto.TaxPercent,
                            itemDto.Quantity,
                            itemDto.TotalWeight,
                            itemDto.CostPrice,
                            itemDto.CostPriceExchangeRate,
                            invoice.PriceUnitId,
                            itemDto.IsInstantProduct,
                            itemDto.Product.Wage,
                            itemDto.Product.WageType,
                            itemDto.Product.WagePriceUnitId.HasValue ? new PriceUnitId(itemDto.Product.WagePriceUnitId.Value) : null,
                            itemDto.WagePriceUnitExchangeRate,
                            itemDto.StonePriceUnitExchangeRate);
                    }
                    else
                    {
                        existingItem.UpdatePurchaseItem(itemDto.GramPrice,
                            itemDto.Quantity,
                            itemDto.TotalWeight,
                            itemDto.CostPrice,
                            itemDto.CostPriceExchangeRate,
                            itemDto.CostPriceUnitId.HasValue
                                ? new PriceUnitId(itemDto.CostPriceUnitId.Value)
                                : null);
                    }

                    existingItem.RecalculateAmounts(product, invoice.InvoiceType);
                }
            }
            else // create
            {
                Product product;

                if (invoice.InvoiceType is InvoiceType.Purchase)
                {
                    // always create the product
                    product = await CreateProductAsync(itemDto.Product, null, cancellationToken);

                    // Add product to invoice
                    invoice.AddPurchaseProductItem(
                        itemDto.Id.HasValue ? new InvoiceProductItemId(itemDto.Id.Value) : null,
                        itemDto.GramPrice,
                        itemDto.ProfitPercent,
                        itemDto.TaxPercent,
                        itemDto.Quantity,
                        itemDto.TotalWeight,
                        itemDto.CostPrice,
                        itemDto.CostPriceExchangeRate,
                        itemDto.StonePriceUnitExchangeRate,
                        itemDto.CostPriceUnitId.HasValue ? new PriceUnitId(itemDto.CostPriceUnitId.Value) : null,
                        itemDto.IsInstantProduct,
                        product);
                }
                else
                {
                    if (itemDto.Product.Id.HasValue)
                    {
                        // existing item: should update product
                        product = await UpdateAsync(new ProductId(itemDto.Product.Id.Value),
                            itemDto.Product,
                            invoice.InvoiceType,
                            cancellationToken);
                    }
                    else
                    {
                        // new product: we should create product
                        product = await CreateProductAsync(itemDto.Product, null, cancellationToken);
                    }

                    // Add product to invoice
                    invoice.AddSaleProductItem(
                        itemDto.Id.HasValue ? new InvoiceProductItemId(itemDto.Id.Value) : null,
                        itemDto.GramPrice,
                        itemDto.ProfitPercent,
                        itemDto.TaxPercent,
                        itemDto.Quantity,
                        itemDto.TotalWeight,
                        itemDto.CostPrice,
                        itemDto.CostPriceExchangeRate,
                        itemDto.CostPriceUnitId.HasValue
                            ? new PriceUnitId(itemDto.CostPriceUnitId.Value)
                            : null,
                        itemDto.IsInstantProduct,
                        itemDto.Product.Wage,
                        itemDto.Product.WageType,
                        itemDto.Product.WagePriceUnitId.HasValue
                            ? new PriceUnitId(itemDto.Product.WagePriceUnitId.Value)
                            : null,
                        itemDto.WagePriceUnitExchangeRate,
                        itemDto.StonePriceUnitExchangeRate,
                        product);
                }
            }
        }
    }

    public Task DeleteRangeAsync(List<Product> productList, CancellationToken cancellationToken = default)
    {
        return repository.DeleteRangeAsync(productList, cancellationToken);
    }

    #endregion
}