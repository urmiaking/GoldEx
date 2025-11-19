using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Models;

public record ExcelProductItem(
    string? Barcode,
    string Name,
    ProductType ProductType,
    decimal Weight,
    WageType WageType,
    decimal Wage,
    string? WagePriceUnit,
    string? ProductCategory,
    decimal Fineness,
    int Quantity = 1);