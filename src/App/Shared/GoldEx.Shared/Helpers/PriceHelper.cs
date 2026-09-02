using System.Globalization;
using System.Text.RegularExpressions;

namespace GoldEx.Shared.Helpers;

public static class PriceHelper
{
    public static double ExtractPercentChange(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return 0;

        var match = Regex.Match(input, @"([-+]?\d+(?:[.,]\d+)?)\s*%");
        if (match.Success && double.TryParse(match.Groups[1].Value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var result))
        {
            return result;
        }

        return 0;
    }
}
