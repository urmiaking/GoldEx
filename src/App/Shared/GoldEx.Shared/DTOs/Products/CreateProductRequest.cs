using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Products;

public record CreateProductRequest(
    Guid Id,
    string Name,
    string Barcode,
    decimal Weight,
    decimal Wage,
    WageType WageType,
    ProductType ProductType,
    CaratType CaratType,
    Guid ProductCategoryId,
    List<CreateGemStoneRequest>? GemStones);