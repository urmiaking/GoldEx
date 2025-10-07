using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Application.Validators.InventoryStocks;

[ScopedService]
internal class MeltUsedProductsValidator : AbstractValidator<List<ProductId>>
{
    private readonly IInventoryStockRepository _inventoryStockRepository;

    public MeltUsedProductsValidator(IInventoryStockRepository inventoryStockRepository)
    {
        _inventoryStockRepository = inventoryStockRepository;

        RuleFor(x => x)
            .NotEmpty().WithMessage("حداقل یک جنس باید انتخاب شود.")
            .MustAsync(NotResultInNegativeInventory)
            .WithMessage("موجودی انبار برای یک یا چندتا از اجناس انتخاب شده کافی نمی باشد.");
    }

    private async Task<bool> NotResultInNegativeInventory(List<ProductId> productIds, CancellationToken cancellationToken = default)
    {
        var currentStock = await _inventoryStockRepository.GetQuantitiesAsync(productIds, cancellationToken);
        return productIds.All(id => currentStock.TryGetValue(id, out var quantity) && quantity > 0);
    }
}