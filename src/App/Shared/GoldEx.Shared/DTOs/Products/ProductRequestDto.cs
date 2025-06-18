using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Products;

/// <summary>
/// Represents a request to create or update a product.
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="Barcode"></param>
/// <param name="Weight"></param>
/// <param name="Wage"></param>
/// <param name="WageType"></param>
/// <param name="ProductType"></param>
/// <param name="CaratType"></param>
/// <param name="ProductCategoryId"></param>
/// <param name="WagePriceUnitId"></param>
/// <param name="GemStones"></param>
public record ProductRequestDto(
    Guid? Id,
    string Name,
    string Barcode,
    decimal Weight,
    decimal Wage,
    WageType WageType,
    ProductType ProductType,
    CaratType CaratType,
    Guid? ProductCategoryId,
    Guid? WagePriceUnitId,
    List<GemStoneRequestDto>? GemStones);