using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Products;

public record GetProductResponse(
    Guid Id,
    string Name,
    string Barcode,
    decimal Weight,
    decimal? Wage,
    ProductType ProductType,
    WageType? WageType,
    CaratType CaratType,
    Guid? ProductCategoryId,
    string? ProductCategoryTitle,
    Guid? WagePriceUnitId,
    string? WagePriceUnitTitle,
    List<GetGemStoneResponse>? GemStones);