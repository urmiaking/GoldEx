using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Products;

namespace GoldEx.Shared.DTOs.InventoryStocks;

public record GetInventoryStockResponse(
    decimal CurrentAmount,
    decimal SoldAmount,
    DateTime DateTime,
    GetProductResponse? Product,
    GetCoinResponse? Coin,
    GetPriceUnitTitleResponse? Currency);