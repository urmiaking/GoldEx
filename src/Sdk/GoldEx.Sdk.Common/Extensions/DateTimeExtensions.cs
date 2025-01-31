using System.Globalization;

namespace GoldEx.Sdk.Common.Extensions;

public static class DateTimeExtensions
{
    //public static DateTime AddMonths(this DateTime date, int months)
    //{
    //    return date.AddMonths(months, CultureInfo.CurrentCulture);
    //}

    public static int GetYear(this DateTime date, CultureInfo culture)
    {
        return date.GetYear(culture.Calendar);
    }

    public static int GetYear(this DateTime date, Calendar calendar)
    {
        return calendar.GetYear(date);
    }

    public static int GetMonth(this DateTime date, CultureInfo culture)
    {
        return date.GetMonth(culture.Calendar);
    }

    public static int GetMonth(this DateTime date, Calendar calendar)
    {
        return calendar.GetMonth(date);
    }

    public static int GetDay(this DateTime date, CultureInfo culture)
    {
        return date.GetDay(culture.Calendar);
    }

    public static int GetDay(this DateTime date, Calendar calendar)
    {
        return calendar.GetDayOfMonth(date);
    }

    public static DateTime SetYear(this DateTime date, int year)
    {
        return date.SetYear(year, CultureInfo.CurrentCulture);
    }

    public static DateTime SetYear(this DateTime date, int year, CultureInfo culture)
    {
        var month = culture.Calendar.GetMonth(date);
        var day = culture.Calendar.GetDayOfMonth(date);
        var daysInMonth = culture.Calendar.GetDaysInMonth(year, month);

        if(day > daysInMonth)
            day = daysInMonth;

        return culture.Calendar.ToDateTime(year, month, day, date.Hour, date.Minute, date.Second, date.Millisecond);
    }

    public static DateTime SetMonth(this DateTime date, int month, CultureInfo culture)
    {
        var year = culture.Calendar.GetYear(date);
        var day = culture.Calendar.GetDayOfMonth(date);
        var daysInMonth = culture.Calendar.GetDaysInMonth(year, month);

        if (day > daysInMonth)
            day = daysInMonth;

        return culture.Calendar.ToDateTime(year, month, day, date.Hour, date.Minute, date.Second, date.Millisecond);
    }

    public static DateTime AddMonths(this DateTime date, int months, CultureInfo culture)
    {
        return culture.Calendar.AddMonths(date, months);
    }

    public static DateTime GetMonthStart(this DateTime date)
    {
        return date.GetMonthStart(CultureInfo.CurrentCulture);
    }

    public static DateTime GetMonthStart(this DateTime date, CultureInfo culture)
    {
        return date.GetMonthStart(culture.Calendar);
    }

    public static DateTime GetMonthStart(this DateTime date, Calendar calendar)
    {
        var year = calendar.GetYear(date);
        var month = calendar.GetMonth(date);
        var day = 1;

        return calendar.ToDateTime(year, month, day, 0, 0, 0, 0);
    }

    public static DateTime GetMonthEnd(this DateTime date)
    {
        return date.GetMonthEnd(CultureInfo.CurrentCulture);
    }

    public static DateTime GetMonthEnd(this DateTime date, CultureInfo culture)
    {
        return date.GetMonthEnd(culture.Calendar);
    }

    public static DateTime GetMonthEnd(this DateTime date, Calendar calendar)
    {
        var year = calendar.GetYear(date);
        var month = calendar.GetMonth(date);
        var day = calendar.GetDaysInMonth(year, month);

        return calendar.ToDateTime(year, month, day, 23, 59, 59, 999);
    }
    
    public static DateTime GetDayStart(this DateTime date)
    {
        return date.Date;
    }
    
    public static DateTime GetDayEnd(this DateTime date)
    {
        return date.Date.Add(new TimeSpan(0, 23, 59, 59, 999));
    }

    public static int CalculateAge(this DateTime birthDate)
    {
        var today = DateTime.UtcNow;

        var age = today.Year - birthDate.Year;

        if (birthDate > today.AddYears(-age))
            age--;

        return age;
    }
}
