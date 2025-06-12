using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Products;

public record UpdateProductRequest(
    string Name,
    string Barcode,
    decimal Weight,
    decimal Wage,
    WageType WageType,
    ProductType ProductType,
    CaratType CaratType,
    Guid? ProductCategoryId,
    Guid? WagePriceUnitId,
    List<UpdateGemStoneRequest>? GemStones);