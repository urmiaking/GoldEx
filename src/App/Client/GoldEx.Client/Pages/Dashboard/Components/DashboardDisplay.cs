using System.Globalization;

namespace GoldEx.Client.Pages.Dashboard.Components;

internal static class DashboardDisplay
{
    private static readonly PersianCalendar Calendar = new();

    public static string Number(decimal number, string format = "#,##0.###") =>
        Digits(number.ToString(format, CultureInfo.InvariantCulture).Replace(',', '٬'));

    public static string Date(DateOnly date, bool includeYear = true)
    {
        var day = date.ToDateTime(TimeOnly.MinValue);
        var monthDay = FormattableString.Invariant($"{Calendar.GetMonth(day):00}/{Calendar.GetDayOfMonth(day):00}");
        return Digits(includeYear ? $"{Calendar.GetYear(day)}/{monthDay}" : monthDay);
    }

    private static string Digits(string text) => string.Concat(text.Select(c => c is >= '0' and <= '9' ? (char)('۰' + c - '0') : c));
}
