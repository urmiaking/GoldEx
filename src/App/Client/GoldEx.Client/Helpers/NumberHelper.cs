namespace GoldEx.Client.Helpers;

public static class NumberHelper
{
    public static string ToCurrencyFormat(this decimal number) => number.ToString("#,##0.##");
}