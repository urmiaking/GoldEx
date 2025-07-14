using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Enums;
using System.Globalization;

namespace GoldEx.Client.Helpers;

public static class NumberHelper
{
    public static string ToCurrencyFormat(this decimal number, string? unit = null)
    {
        if (string.IsNullOrEmpty(unit))
            return number.ToString("#,##0.##");

        if (unit == "ریال")
            return number.ToString("#,##0") + " " + unit;

        return number.ToString("#,##0.##") + " " + unit;
    }

    public static string ToWeightFormat(this decimal number, GoldUnitType unitType)
    {
        return $"{number:G29} {unitType.GetDisplayName()}";
    }

    public static string ToCurrencyReportFormat(this decimal number, string? unit = null)
    {
        var nfi = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();

        nfi.NumberGroupSeparator = "،"; 

        if (string.IsNullOrEmpty(unit))
            return number.ToString("#,##0.##", nfi);

        if (unit == "ریال")
        {
            return number.ToString("#,##0", nfi) + " " + unit;
        }

        return number.ToString("#,##0.##", nfi) + " " + unit;
    }
}