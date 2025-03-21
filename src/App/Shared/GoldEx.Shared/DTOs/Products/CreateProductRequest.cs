using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Products;

public record CreateProductRequest(
    Guid Id,
    string Name,
    string Barcode,
    double Weight,
    double? Wage,
    WageType? WageType,
    ProductType ProductType,
    CaratType CaratType,
    Guid ProductCategoryId,
    List<CreateGemStoneRequest>? GemStones);