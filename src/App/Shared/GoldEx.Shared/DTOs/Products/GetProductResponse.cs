using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Products;

public record GetProductResponse(
    Guid Id,
    string Name,
    string Barcode,
    double Weight,
    double? Wage,
    ProductType ProductType,
    WageType? WageType,
    CaratType CaratType,
    Guid ProductCategoryId,
    string ProductCategoryTitle,
    List<GetGemStoneResponse>? GemStones);