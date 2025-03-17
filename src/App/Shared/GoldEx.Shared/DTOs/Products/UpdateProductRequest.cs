using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Products;

public record UpdateProductRequest(
    string Name,
    string Barcode,
    double Weight,
    double? Wage,
    WageType? WageType,
    ProductType ProductType,
    CaratType CaratType,
    Guid ProductCategoryId);