using MudBlazor;

namespace GoldEx.Client.Extensions;

public static class DateRangeExtensions
{
    public static DateRange? From(DateTime? start, DateTime? end)
        => start == null && end == null ? null : new DateRange(start, end);
}