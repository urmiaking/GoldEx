using GoldEx.Shared.DTOs.Coins;
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
    GetProductResponse? Product,
    GetCoinResponse? Coin,
    GetPriceUnitTitleResponse? Currency)
{
    public decimal FinalPrice { get; set; }
}