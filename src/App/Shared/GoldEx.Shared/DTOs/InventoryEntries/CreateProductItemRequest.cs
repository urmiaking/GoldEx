using GoldEx.Shared.DTOs.Products;

namespace GoldEx.Shared.DTOs.InventoryEntries;

public record CreateProductItemRequest(
    decimal Quantity,
    decimal UnitPrice,
    decimal CostPrice,
    Guid CostPriceUnitId,
    decimal? CostPriceExchangeRate,
    decimal? WagePriceUnitExchangeRate,
    decimal? StonePriceUnitExchangeRate,
    ProductRequestDto Product);