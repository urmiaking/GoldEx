using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Products;

public record GetPendingProductResponse(Guid Id,
    string Name,
    string Barcode,
    double Weight,
    double? Wage,
    ProductType ProductType,
    WageType? WageType,
    CaratType CaratType,
    Guid ProductCategoryId,
    DateTime LastModifiedDate,
    ModifyStatus? Status,
    bool? IsDeleted,
    List<GetGemStoneResponse>? GemStones);