using FluentValidation;
using FluentValidation.Results;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.InventoryEntries;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.Products;

[ScopedService]
internal class UpdateProductRequestValidator : AbstractValidator<ProductRequestDto>
{
    private readonly IInventoryEntryRepository _inventoryEntryRepository;
    private readonly IInventoryStockRepository _inventoryStockRepository;
    private readonly IBarcodeGeneratorService _barcodeGeneratorService;
    private readonly IProductRepository _productRepository;
    public UpdateProductRequestValidator(
        IInventoryEntryRepository inventoryEntryRepository,
        IInventoryStockRepository inventoryStockRepository, 
        IBarcodeGeneratorService barcodeGeneratorService, 
        IProductRepository productRepository)
    {
        _inventoryEntryRepository = inventoryEntryRepository;
        _inventoryStockRepository = inventoryStockRepository;
        _barcodeGeneratorService = barcodeGeneratorService;
        _productRepository = productRepository;

        RuleFor(x => x)
            .MustAsync(BeInventoryEntryRecord)
            .WithMessage("تنها اجناسی که به صورت دستی وارد انبار شده اند قابلیت ویرایش دارند")
            .MustAsync(NotResultInNegativeInventory)
            .WithMessage("ویرایش این جنس منجر به منفی شدن موجودی انبار می‌شود");

        RuleFor(x => x.Barcode)
            .NotEmpty()
            .WithMessage("بارکد نمی‌تواند خالی باشد")
            .MustAsync(BeUniqueBarcode)
            .WithMessage("بارکد وارد شده تکراری است");
    }

    private async Task<bool> BeUniqueBarcode(ProductRequestDto productDto, string? barcode, CancellationToken cancellationToken)
    {
        if (!productDto.Id.HasValue)
            return false;

        var product = await _productRepository
            .Get(new ProductsByIdSpecification(new ProductId(productDto.Id.Value)))
            .FirstOrDefaultAsync(cancellationToken);

        if (product == null)
            return false;

        if (product.Barcode == barcode)
            return true;

        if (string.IsNullOrEmpty(barcode))
            return true; // Handled by NotEmpty rule

        try
        {
            await _barcodeGeneratorService.ValidateUniquenessAsync(BarcodeType.Product, barcode, cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task<bool> BeInventoryEntryRecord(ProductRequestDto productDto, CancellationToken cancellationToken)
    {
        if (!productDto.Id.HasValue)
            return false;

        var isExistInInventoryEntry = await _inventoryEntryRepository.ExistsAsync(
            new InventoryEntriesByProductIdSpecification(new ProductId(productDto.Id.Value)),
            cancellationToken);

        return isExistInInventoryEntry;
    }

    private async Task<bool> NotResultInNegativeInventory(ProductRequestDto productDto, CancellationToken cancellationToken)
    {
        // If it's a new product, no inventory history exists, so it's safe.
        if (!productDto.Id.HasValue)
            return false;

        var productId = new ProductId(productDto.Id.Value);

        // 1. Get the Current Net Balance (e.g., 4g)
        var currentStock = await _inventoryStockRepository.GetQuantityAsync(productId, cancellationToken);

        // 2. Get the Old Weight from the Database (e.g., 10g)
        // Assuming you have a way to get the existing entity via repository
        var originalProduct = await _productRepository
            .Get(new ProductsByIdSpecification(productId))
            .AsNoTracking() // Important: Don't track this, or it might conflict with the update later
            .FirstOrDefaultAsync(cancellationToken);

        if (originalProduct == null)
            return false; // Product not found

        // 3. Calculate how much has been "Used" (Sold, Melted, etc.)
        // Used = 10g (Total) - 4g (Remaining) = 6g
        var usedAmount = originalProduct.Weight - currentStock;

        // 4. Check if the New Weight covers the Used Amount
        // If New Weight is 5g: 5 < 6 => FALSE (Result would be -1g)
        // If New Weight is 7g: 7 >= 6 => TRUE (Result would be 1g)
        if (productDto.Weight < usedAmount)
        {
            return false; // Validation Failed: New weight is too low for current usage history
        }

        return true; // Validation Passed
    }
}