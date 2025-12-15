using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Models;

public record InventorySummaryData
{
    public Product? Product { get; init; }
    public decimal? SaleWage { get; init; }
    public WageType? SaleWageType { get; init; }
    public string? SaleWagePriceUnitTitle { get; init; }

    public decimal? PurchaseWage { get; init; }
    public WageType? PurchaseWageType { get; init; }
    public string? PurchaseWagePriceUnitTitle { get; init; }

    public Coin? Coin { get; init; }
    public PriceUnit? Currency { get; init; }
    public required decimal CurrentAmount { get; init; }
    public required decimal SoldAmount { get; init; }
    public DateTime DateTime { get; set; }
}