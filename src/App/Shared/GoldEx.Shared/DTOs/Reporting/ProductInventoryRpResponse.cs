using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Reporting;

public record ProductInventoryRpResponse(
    DateTime DateTime,
    GetProductResponse Product,
    decimal CurrentAmount,
    decimal SoldAmount,
    decimal? SaleWage,
    WageType? SaleWageType,
    string? SaleWagePriceUnitTitle,
    decimal? PurchaseWage,
    WageType? PurchaseWageType,
    string? PurchaseWagePriceUnitTitle);