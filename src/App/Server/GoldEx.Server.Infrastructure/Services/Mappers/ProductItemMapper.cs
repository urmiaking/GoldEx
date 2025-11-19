using GoldEx.Sdk.Common.Extensions;
using GoldEx.Server.Infrastructure.Models;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using GoldEx.Shared.Enums;
using System.Globalization;

namespace GoldEx.Server.Infrastructure.Services.Mappers;

public sealed class ExcelProductItemMapper : IExcelRowMapper<ExcelProductItem>
{
    public IReadOnlyCollection<string> HeaderHints { get; } =
    [
        "barcode", "بارکد", "کد",
        "name", "نام کالا", "عنوان", "شرح",
        "type", "نوع", "نوع کالا",
        "weight", "وزن",
        "wage", "اجرت",
        "wageType", "نوع اجرت",
        "category", "دسته بندی",
        "fineness", "عیار",
        "quantity", "تعداد"
    ];

    public bool TryMap(
        IReadOnlyDictionary<string, string?> row,
        int rowIndex,
        out ExcelProductItem? item,
        out string? reason)
    {
        item = null;
        reason = null;

        // Helper local to normalize strings
        static string Norm(string? x) => (x ?? "").Trim().ToPersianChars().ToLowerInvariant();

        string? Find(params string[] keys)
        {
            foreach (var key in keys)
            {
                var match = row.FirstOrDefault(k => Norm(k.Key).Contains(Norm(key)));
                if (!string.IsNullOrWhiteSpace(match.Value))
                    return match.Value;
            }
            return null;
        }

        // Extracting fields
        var barcode = Find("barcode", "بارکد", "کد");
        var name = Find("name", "نام", "title", "عنوان", "شرح");
        var typeText = Find("نوع", "type", "نوع کالا");
        var weightText = Find("weight", "وزن");
        var wageText = Find("wage", "اجرت");
        var wageTypeText = Find("نوع اجرت", "wage type");
        var category = Find("category", "دسته");
        var finenessText = Find("عیار", "fineness");
        var qtyText = Find("تعداد", "qty", "quantity");

        // Name required
        if (string.IsNullOrWhiteSpace(name))
        {
            reason = "نام کالا خالی است";
            return false;
        }

        // Parse type
        ProductType type;
        var nt = Norm(typeText);
        if (nt.Contains("جواهر")) type = ProductType.Jewelry;
        else if (nt.Contains("طلا") && !nt.Contains("آب")) type = ProductType.Gold;
        else if (nt.Contains("آب")) type = ProductType.MoltenGold;
        else type = ProductType.Gold; // default fallback

        // Parse weight
        if (!decimal.TryParse(weightText, NumberStyles.Any, CultureInfo.InvariantCulture, out var weight))
        {
            reason = "وزن معتبر نیست";
            return false;
        }

        // Parse wage
        decimal wage = 0;
        if (!string.IsNullOrWhiteSpace(wageText))
        {
            decimal.TryParse(wageText, NumberStyles.Any, CultureInfo.InvariantCulture, out wage);
        }

        // Determine wage type
        WageType wageType;
        string? wageUnit = null;
        var wtNorm = Norm(wageTypeText);

        if (!string.IsNullOrWhiteSpace(wageTypeText))
        {
            // Header exists → detect textual
            if (wtNorm.Contains("درصد"))
            {
                wageType = WageType.Percent;
            }
            else
            {
                wageType = WageType.Fixed;
                wageUnit = wageTypeText.Trim();
            }
        }
        else
        {
            // Header missing → auto-detect
            if (wage > 100)
            {
                wageType = WageType.Fixed;
                wageUnit = "تومان";
            }
            else
            {
                wageType = WageType.Percent;
            }
        }

        // Parse fineness (maybe 750 or 18)
        decimal fineness = 750;
        if (!string.IsNullOrWhiteSpace(finenessText))
        {
            if (decimal.TryParse(finenessText, out var fin))
            {
                fineness = fin > 100 ? fin : (fin / 24m) * 1000m;
            }
        }

        // Quantity
        int qty = 1;
        if (!string.IsNullOrWhiteSpace(qtyText) && int.TryParse(qtyText, out var qtyParsed))
            qty = qtyParsed;

        // Build record
        item = new ExcelProductItem(
            Barcode: string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim(),
            Name: name.Trim(),
            ProductType: type,
            Weight: weight,
            WageType: wageType,
            Wage: wage,
            WagePriceUnit: wageUnit,
            ProductCategory: string.IsNullOrWhiteSpace(category) ? null : category.Trim(),
            Fineness: fineness,
            Quantity: qty
        );

        return true;
    }
}