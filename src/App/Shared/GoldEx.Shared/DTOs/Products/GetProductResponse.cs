using GoldEx.Shared.DTOs.PriceUnits;
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
    decimal Fineness,
    Guid? ProductCategoryId,
    string? ProductCategoryTitle,
    Guid? WagePriceUnitId,
    string? WagePriceUnitTitle,
    DateTime DateTime,
    GoldUnitType GoldUnitType,
    GetPriceUnitTitleResponse? StonePriceUnit,
    List<GetGemStoneResponse>? GemStones,
    GetMoltenGoldResponse? MoltenGold);