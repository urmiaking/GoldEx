using System.Globalization;

namespace GoldEx.Sdk.Common.Extensions;

public static class DateOnlyExtensions
{
    public static int GetYear(this DateOnly date, CultureInfo culture)
    {
        return date.GetYear(culture.Calendar);
    }
    public static int GetYear(this DateOnly date, Calendar calendar)
    {
        var dateTime = date.ToDateTime(TimeOnly.MinValue);

        return dateTime.GetYear(calendar);
    }

    public static int GetMonth(this DateOnly date, CultureInfo culture)
    {
        return date.GetMonth(culture.Calendar);
    }
    public static int GetMonth(this DateOnly date, Calendar calendar)
    {
        var dateTime = date.ToDateTime(TimeOnly.MinValue);

        return dateTime.GetMonth(calendar);
    }

    public static int GetDay(this DateOnly date, CultureInfo culture)
    {
        return date.GetDay(culture.Calendar);
    }
    public static int GetDay(this DateOnly date, Calendar calendar)
    {
        var dateTime = date.ToDateTime(TimeOnly.MinValue);

        return dateTime.GetDay(calendar);
    }

    public static DateOnly SetYear(this DateOnly date, int year)
    {
        return date.SetYear(year, CultureInfo.CurrentCulture);
    }

    public static DateOnly SetYear(this DateOnly date, int year, CultureInfo culture)
    {
        var dateTime = date.ToDateTime(TimeOnly.MinValue);

        return DateOnly.FromDateTime(dateTime.SetYear(year, culture));
    }

    public static DateOnly SetMonth(this DateOnly date, int month, CultureInfo culture)
    {
        var dateTime = date.ToDateTime(TimeOnly.MinValue);

        return DateOnly.FromDateTime(dateTime.SetMonth(month, culture));
    }

    public static DateOnly AddMonths(this DateOnly date, int months, CultureInfo culture)
    {
        var dateTime = date.ToDateTime(TimeOnly.MinValue);
        return DateOnly.FromDateTime(dateTime.AddMonths(months, culture));
    }

    public static DateOnly GetMonthStart(this DateOnly date)
    {
        return date.GetMonthStart(CultureInfo.CurrentCulture);
    }

    public static DateOnly GetMonthStart(this DateOnly date, CultureInfo culture)
    {
        return date.GetMonthStart(culture.Calendar);
    }

    public static DateOnly GetMonthStart(this DateOnly date, Calendar calendar)
    {
        var dateTime = date.ToDateTime(TimeOnly.MinValue);

        return DateOnly.FromDateTime(dateTime.GetMonthStart(calendar));
    }

    public static DateOnly GetMonthEnd(this DateOnly date)
    {
        return date.GetMonthEnd(CultureInfo.CurrentCulture);
    }

    public static DateOnly GetMonthEnd(this DateOnly date, CultureInfo culture)
    {
        return date.GetMonthEnd(culture.Calendar);
    }

    public static DateOnly GetMonthEnd(this DateOnly date, Calendar calendar)
    {
        var dateTime = date.ToDateTime(TimeOnly.MinValue);

        return DateOnly.FromDateTime(dateTime.GetMonthEnd(calendar));
    }

    public static int CalculateAge(this DateOnly birthDate)
    {
        var dateTime = birthDate.ToDateTime(TimeOnly.MinValue);
        return dateTime.CalculateAge();
    }
}
