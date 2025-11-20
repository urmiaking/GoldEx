using GoldEx.Shared.DTOs.Products;

namespace GoldEx.Shared.DTOs.InventoryEntries;

public record GetProductItemResponse(GetProductResponse Product, int Quantity, decimal? WagePriceUnitExchangeRate, Guid CostPriceUnitId);