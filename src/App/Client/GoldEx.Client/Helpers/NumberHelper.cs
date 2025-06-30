namespace GoldEx.Client.Helpers;

public static class NumberHelper
{
    public static string ToCurrencyFormat(this decimal number, string? unit = null)
    {
        if (string.IsNullOrEmpty(unit))
        {
            return number.ToString("#,##0.##");
        }

        if (unit == "ریال")
        {
            return number.ToString("#,##0") + " " + unit;
        }

        return number.ToString("#,##0.##") + " " + unit;
    }
}