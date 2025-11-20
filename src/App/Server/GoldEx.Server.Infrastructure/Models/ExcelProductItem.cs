using GoldEx.Sdk.Common.Extensions;
using GoldEx.Server.Infrastructure.Models.Ai;
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
    int Quantity = 1)
{
    public CategoryModelInput ToAiInput()
    {
        return new CategoryModelInput
        {
            Barcode = ToBarcode(Barcode),
            Name = Name,
            ProductType = ProductType.GetDisplayName(),
            Weight = (float) Weight,
            Fineness = FinenessToKarat(Fineness),
            WageType = WageType is WageType.Percent ? WageType.GetDisplayName() : WagePriceUnit ?? string.Empty,
            Wage = (float) Wage
        };
    }

    private float FinenessToKarat(decimal fineness)
    {
        if (fineness <= 24)
            return (float) fineness;

        return (float)((fineness / 1000m) * 24m);
    }

    private float ToBarcode(string? barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return 0;

        if (long.TryParse(barcode, out var result))
            return result;

        return 0;
    }
}