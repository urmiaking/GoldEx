using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Infrastructure.Models;

public record InventorySummaryData
{
    public Product? Product { get; init; }
    public Coin? Coin { get; init; }
    public PriceUnit? Currency { get; init; }
    public required decimal CurrentQuantity { get; init; }
}