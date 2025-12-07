using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Coins;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.DTOs.InventoryExits;

namespace GoldEx.Server.Application.Validators.InventoryExits;

[ScopedService]
internal class CreateInventoryExitRequestValidator : AbstractValidator<CreateInventoryExitRequest>
{
    private readonly IInventoryStockRepository _inventoryStockRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICoinRepository _coinRepository;
    public CreateInventoryExitRequestValidator(IInventoryStockRepository inventoryStockRepository, IProductRepository productRepository, ICoinRepository coinRepository)
    {
        _inventoryStockRepository = inventoryStockRepository;
        _productRepository = productRepository;
        _coinRepository = coinRepository;

        RuleForEach(x => x.Products)
            .Must(x => x.Weight > 0)
            .WithMessage((_, item) => $"وزن محصول با شناسه {item.ProductId} باید بزرگتر از صفر باشد")
            .MustAsync(BeValidProduct)
            .WithMessage((_, item) => $"محصول با شناسه {item.ProductId} یافت نشد")
            .MustAsync(NotResultInNegativeProductInventory)
            .WithMessage((_, item) => $"وزن {item.Weight:G29} بیشتر از موجودی انبار می باشد");

        RuleForEach(x => x.Coins)
            .Must(x => x.Quantity > 0)
            .WithMessage((_, item) => $"تعداد سکه با شناسه {item.CoinId} باید بزرگتر از صفر باشد")
            .MustAsync(BeValidCoin)
            .WithMessage((_, item) => $"سکه با شناسه {item.CoinId} یافت نشد")
            .MustAsync(NotResultInNegativeCoinInventory)
            .WithMessage((_, item) => $"تعداد {item.Quantity} بیشتر از موجودی انبار می باشد");
    }

    private Task<bool> BeValidProduct(CreateProductItemExitRequest item, CancellationToken cancellationToken = default)
    {
        return _productRepository.ExistsAsync(new ProductsByIdSpecification(new ProductId(item.ProductId)), cancellationToken);
    }

    private async Task<bool> NotResultInNegativeProductInventory(CreateProductItemExitRequest item, CancellationToken cancellationToken = default)
    {
        var currentStock = await _inventoryStockRepository.GetQuantityAsync(new ProductId(item.ProductId), cancellationToken);
        return currentStock >= item.Weight;
    }

    private Task<bool> BeValidCoin(CreateCoinItemExitRequest item, CancellationToken cancellationToken = default)
    {
        return _coinRepository.ExistsAsync(new CoinsByIdSpecification(new CoinId(item.CoinId)), cancellationToken);
    }

    private async Task<bool> NotResultInNegativeCoinInventory(CreateCoinItemExitRequest item, CancellationToken cancellationToken = default)
    {
        var currentStock = await _inventoryStockRepository.GetQuantityAsync(new CoinId(item.CoinId), cancellationToken);
        return currentStock >= item.Quantity;
    }
}