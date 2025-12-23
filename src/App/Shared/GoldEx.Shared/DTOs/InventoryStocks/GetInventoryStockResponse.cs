using GoldEx.Shared.DTOs.CoinInstances;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.InventoryStocks;

public record GetInventoryStockResponse(
    decimal CurrentAmount,
    decimal SoldAmount,
    DateTime DateTime,
    decimal? SaleWage,
    WageType? SaleWageType,
    string? SaleWagePriceUnitTitle,
    decimal? PurchaseWage,
    WageType? PurchaseWageType,
    string? PurchaseWagePriceUnitTitle,
    GetProductResponse? Product,
    GetCoinInstanceResponse? Coin,
    GetPriceUnitTitleResponse? Currency)
{
    // Parameterless constructor for mapping purposes
    public GetInventoryStockResponse() : this(0, 0, default, null, null, null, null, null, null, null, null, null) { }

    public decimal FinalPrice { get; set; }
}