using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.InventoryStocks;

public record CalculatorFilterRequest(
    decimal GramPrice,
    decimal TaxPercent,
    decimal ProfitPercent,
    decimal? Fineness,
    decimal? MaxWage,
    decimal? MinWeight,
    decimal? MaxWeight,
    decimal? MinPrice,
    decimal? MaxPrice,
    Guid? ProductCategoryId,
    ProductType ProductType
);