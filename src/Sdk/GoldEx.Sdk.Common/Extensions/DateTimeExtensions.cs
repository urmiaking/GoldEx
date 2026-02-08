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
        var today = DateTime.Now;

        var age = today.Year - birthDate.Year;

        if (birthDate > today.AddYears(-age))
            age--;

        return age;
    }

    public static DateTime ToGregorianDateTime(this string persianDateTime)
    {
        if (string.IsNullOrWhiteSpace(persianDateTime))
            throw new ArgumentNullException(nameof(persianDateTime));

        var input = persianDateTime.Trim();
        var pc = new PersianCalendar();

        // Split parts
        var parts = input.Split(' ');
        string datePart;
        string? timePart = null;

        // Detect format:
        // "1404/08/18 18:07" or "18:07 1404/08/18"
        if (parts.Length == 2)
        {
            if (parts[0].Contains("/"))
            {
                datePart = parts[0];
                timePart = parts[1];
            }
            else if (parts[1].Contains("/"))
            {
                datePart = parts[1];
                timePart = parts[0];
            }
            else
            {
                throw new FormatException("Invalid Persian datetime format.");
            }
        }
        else if (parts.Length == 1 && parts[0].Contains("/"))
        {
            datePart = parts[0];
        }
        else
        {
            throw new FormatException("Invalid Persian datetime format.");
        }

        // Parse date
        var date = datePart.Split('/');
        if (date.Length != 3)
            throw new FormatException("Invalid Persian date format.");

        var year = int.Parse(date[0]);
        var month = int.Parse(date[1]);
        var day = int.Parse(date[2]);

        // Parse time (optional)
        int hour = 0, minute = 0, second = 0;
        if (!string.IsNullOrEmpty(timePart))
        {
            var time = timePart.Split(':');
            hour = int.Parse(time[0]);
            minute = int.Parse(time[1]);
            if (time.Length > 2)
                second = int.Parse(time[2]);
        }

        return pc.ToDateTime(year, month, day, hour, minute, second, 0);
    }

    public static bool TryToGregorianDateTime(
        this string? persianDateTime,
        out DateTime result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(persianDateTime))
            return false;

        try
        {
            result = persianDateTime.ToGregorianDateTime();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
