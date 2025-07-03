using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GoldEx.Sdk.Common.Extensions;

public static class StringExtensions
{
    public static string FormatRoute(this string template) => template.FormatRoute(new { });
    
    public static string FormatRoute(this string template, object parameters)
    {
        var route = template.RichFormat(parameters);
        route = route.Replace("//", "/");

        return route;
    }

    public static string RichFormat(this string template, object parameters)
    {
        var properties = parameters.GetType().GetProperties();
        var dictionary = properties.ToDictionary(property => property.Name, property => property.GetValue(parameters));

        return template.RichFormat(dictionary);
    }

    public static string RichFormat(this string template, Dictionary<string, object?> parameters)
    {
        var matches = Regex.Matches(template, "{(.*?)}");
        foreach (Match match in matches)
        {
            var valueWithoutBrackets = match.Groups[1].Value;
            var valueWithBrackets = match.Value;
            var isNullable = valueWithoutBrackets.EndsWith('?');

            var argName = valueWithoutBrackets.Split(':')[0]; // remove second part for params of format {id:int}

            if (isNullable && argName.EndsWith('?'))
                argName = argName[..^1];

            var key = parameters.Keys.FirstOrDefault(x => string.Compare(x, argName, StringComparison.OrdinalIgnoreCase) == 0);
            if (key != null)
            {
                var value = parameters[key];

                template = template.Replace(valueWithBrackets, value?.ToString());
            }
            else if(isNullable)
            {
                template = template.Replace(valueWithBrackets, string.Empty);
            }
            else
            {
                throw new ArgumentException($"The '{argName}' argument is required.");
            }
        }

        return template;
    }

    public static string AppendQueryString(this string template, object? parameters)
    {
        if (parameters is null)
            return template;

        var dictionary = new Dictionary<string, object?>();

        var props = parameters.GetType().GetProperties();
        foreach (var prop in props)
        {
            var value = prop.GetValue(parameters);
            if (value == null) continue;

            if (value is IEnumerable enumerable and not string)
            {
                var list = enumerable.Cast<object?>().ToList();

                dictionary[prop.Name] = list;
            }
            else
            {
                dictionary[prop.Name] = value;
            }
        }

        return template.AppendQueryString(dictionary);
    }

    public static string AppendQueryString(this string template, Dictionary<string, object?> parameters)
    {
        string url;
        var query = string.Empty;

        if (template.Contains('?'))
        {
            var splits = template.Split('?');
            url = splits[0];
            query = splits[1];
        }
        else
        {
            url = template;
        }

        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(query))
        {
            queryParams.AddRange(query.Split('&'));
        }

        foreach (var kvp in parameters)
        {
            if (kvp.Value is IEnumerable enumerable && kvp.Value is not string)
            {
                foreach (var item in enumerable)
                {
                    var val = item switch
                    {
                        DateTime dateTime => dateTime.ToString(new CultureInfo("en-US")),
                        DateOnly date => date.ToString(new CultureInfo("en-US")),
                        _ => item?.ToString()
                    };
                    if (val != null)
                        queryParams.Add($"{kvp.Key}={Uri.EscapeDataString(val)}");
                }
            }
            else
            {
                var val = kvp.Value switch
                {
                    DateTime dateTime => dateTime.ToString(new CultureInfo("en-US")),
                    DateOnly date => date.ToString(new CultureInfo("en-US")),
                    _ => kvp.Value?.ToString()
                };
                if (val != null)
                    queryParams.Add($"{kvp.Key}={Uri.EscapeDataString(val)}");
            }
        }

        return $"{url}?{string.Join("&", queryParams)}";
    }

    public static string ToPersianChars(this string input)
    {
        return string.IsNullOrEmpty(input) ? input : input.Replace('ك', 'ک').Replace('ي', 'ی');
    }

    public static string ToEnglishNumericsChars(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return input.Replace('۰', '0')
            .Replace('۱', '1')
            .Replace('۲', '2')
            .Replace('۳', '3')
            .Replace('۴', '4')
            .Replace('۵', '5')
            .Replace('۶', '6')
            .Replace('۷', '7')
            .Replace('۸', '8')
            .Replace('۹', '9')
            ;
    }

    public static string FormatDateString(this string dateStr)
    {
        if (string.IsNullOrEmpty(dateStr) || dateStr.Length != 8)
            return string.Empty;

        return $"{dateStr[..4]}/{dateStr.Substring(4, 2)}/{dateStr.Substring(6, 2)}";
    }

    public static string GetInitials(this string name)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;

        // Handle both English and Arabic names correctly.
        var names = Regex.Split(name.Trim(), @"\s+"); // Split by any whitespace

        return names.Length == 0 ? "" : names.Where(n => !string.IsNullOrEmpty(n)).Aggregate("", (current, n) => current + n[..1]);
    }

    public static string GetInitialsWithPeriods(this string name)
    {
        var initials = GetInitials(name);
        return string.Join(".", initials.ToCharArray()); // Add periods
    }

    public static string GenerateRandomBarcode()
    {
        var randomNumber = IntExtensions.GenerateRandomNumber(10000000, 99999999);
        return randomNumber.ToString();
    }

    public static string GenerateRandomCode(int digits)
    {
        if (digits <= 0)
            throw new ArgumentOutOfRangeException(nameof(digits), "Number of digits must be positive.");

        var minValue = (int)Math.Pow(10, digits - 1);
        var maxValue = (int)Math.Pow(10, digits) - 1;

        long randomNumber = IntExtensions.GenerateRandomNumber(minValue, maxValue);

        return randomNumber.ToString();
    }
}