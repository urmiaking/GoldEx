using System.Globalization;

namespace GoldEx.Server.Components.Invoices;

internal static class InvoicePrintFormat
{
    private static readonly CultureInfo Persian = CultureInfo.GetCultureInfo("fa-IR");
    private static readonly NumberFormatInfo Numbers = new()
    {
        NumberGroupSeparator = "٬",
        NumberDecimalSeparator = "٫",
        NegativeSign = "−",
    };

    public static string Number(decimal value) => Digits(value.ToString("#,##0.##########", Numbers));

    public static string Weight(decimal value) => Digits(value.ToString("#,##0.###", Numbers));

    public static string Amount(decimal value, string? unit)
    {
        var title = unit?.Trim();
        if (title is "تومان" or "ریال")
            return Digits(value.ToString("#,##0", Numbers));

        var isWeight = title == "آبشده نقدی" || title?.Contains("گرم", StringComparison.Ordinal) == true ||
            title?.Contains("مثقال", StringComparison.Ordinal) == true;
        return isWeight ? Weight(value) : Number(value);
    }

    public static string Date(DateOnly value) => value < DateOnly.FromDateTime(Persian.Calendar.MinSupportedDateTime)
        ? "—"
        : Digits(value.ToString("yyyy'/'MM'/'dd", Persian));

    public static string Digits(string value) => string.Create(value.Length, value, static (buffer, input) =>
    {
        for (var index = 0; index < input.Length; index++)
        {
            buffer[index] = input[index] switch
            {
                >= '0' and <= '9' => (char)('۰' + input[index] - '0'),
                >= '٠' and <= '٩' => (char)('۰' + input[index] - '٠'),
                _ => input[index],
            };
        }
    });
}
