namespace GoldEx.Client.Helpers;

public static class NumberHelper
{
    public static string FormatNumber(this decimal? number)
    {
        if (!number.HasValue)
            return string.Empty;

        return number.Value.ToString(number.Value % 1 == 0 ?
            "N0" :
            "#,##0.#############################");
    }
}