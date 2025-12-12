using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.InventoryStocks;

namespace GoldEx.Server.Application.Validators.Coins;

[ScopedService]
internal class DeleteCoinValidator : AbstractValidator<Guid>
{
    private readonly IInventoryStockRepository _inventoryStockRepository;
    public DeleteCoinValidator(IInventoryStockRepository inventoryStockRepository)
    {
        _inventoryStockRepository = inventoryStockRepository;

        RuleFor(x => x)
            .MustAsync(NotUsedInInventories)
            .WithMessage("امکان حذف این سکه به دلیل مورد استفاده قرار گرفتن وجود ندارد.");
    }

    private async Task<bool> NotUsedInInventories(Guid coinId, CancellationToken cancellationToken)
    {
        var exists = await _inventoryStockRepository.ExistsAsync(new InventoryStocksByCoinIdSpecification(new CoinId(coinId)), cancellationToken);
        return !exists;
    }
}