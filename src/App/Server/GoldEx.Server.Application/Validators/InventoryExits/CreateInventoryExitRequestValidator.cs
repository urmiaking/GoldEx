using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.CoinInstanceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Coins;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.DTOs.InventoryExits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;

namespace GoldEx.Server.Application.Validators.InventoryExits;

[ScopedService]
internal class CreateInventoryExitRequestValidator : AbstractValidator<CreateInventoryExitRequest>
{
    private readonly IInventoryStockRepository _inventoryStockRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICoinRepository _coinRepository;
    private readonly IPriceUnitRepository _priceUnitRepository;
    private readonly ITransactionService _transactionService;
    public CreateInventoryExitRequestValidator(IInventoryStockRepository inventoryStockRepository,
        IProductRepository productRepository,
        ICoinRepository coinRepository,
        IPriceUnitRepository priceUnitRepository,
        ITransactionService transactionService)
    {
        _inventoryStockRepository = inventoryStockRepository;
        _productRepository = productRepository;
        _coinRepository = coinRepository;
        _priceUnitRepository = priceUnitRepository;
        this._transactionService = transactionService;

        RuleForEach(x => x.Products)
            .Must(x => x.Weight > 0)
            .WithMessage("وزن محصول باید بزرگتر از صفر باشد")
            .MustAsync(BeValidProduct)
            .WithMessage("محصول معتبر نمی باشد")
            .MustAsync(NotResultInNegativeProductInventory)
            .WithMessage((_, item) => $"وزن محصول {item.Weight.ToWeightFormat(GoldUnitType.Gram)} بیشتر از موجودی انبار می باشد");

        RuleForEach(x => x.Coins)
            .Must(x => x.Quantity > 0)
            .WithMessage("تعداد سکه باید بزرگتر از صفر باشد")
            .MustAsync(NotResultInNegativeCoinInventory)
            .WithMessage((_, item) => $"تعداد {item.Quantity} سکه بیشتر از موجودی انبار می باشد");

        RuleForEach(x => x.Currencies)
            .Must(x => x.Quantity > 0)
            .WithMessage("حجم ارز باید بزرگتر از صفر باشد")
            .MustAsync(BeValidCurrency)
            .WithMessage("ارز معتبر نمی باشد")
            .MustAsync(NotResultInNegativeCurrencyAccount)
            .WithMessage((_, item) => $"حجم وارد شده {item.Quantity} ارز بیشتر از موجودی حساب می باشد");
    }

    private Task<bool> BeValidCurrency(CreateCurrencyItemExitRequest item, CancellationToken cancellationToken = default)
    {
        return _priceUnitRepository.ExistsAsync(new PriceUnitsByIdSpecification(new PriceUnitId(item.Id)), cancellationToken);
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

    private async Task<bool> NotResultInNegativeCoinInventory(CreateCoinItemExitRequest item, CancellationToken cancellationToken = default)
    {
        var currentStock = await _inventoryStockRepository.GetQuantityAsync(new CoinInstanceId(item.Id), cancellationToken);
        return currentStock >= item.Quantity;
    }

    private async Task<bool> NotResultInNegativeCurrencyAccount(CreateCurrencyItemExitRequest item, CancellationToken cancellationToken = default)
    {
        var accountBalance = await _transactionService.GetFinancialAccountBalanceAsync(item.FinancialAccountId, cancellationToken);
        return accountBalance.Amount >= item.Quantity;
    }
}