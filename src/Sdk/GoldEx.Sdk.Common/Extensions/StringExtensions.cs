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

    public static string AppendQueryString(this string template, object parameters)
    {
        var properties = parameters.GetType().GetProperties();
        var dictionary = new Dictionary<string, object?>();
        foreach (var property in properties)
        {
            var value = property.GetValue(parameters);
            if (value != null)
                dictionary.Add(property.Name, value);
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

        var queryParams = new Dictionary<string, string?>();
        foreach (var arg in query.Split('&'))
        {
            var argSplit = arg.Split('=');
            var argName = argSplit.Any() ? argSplit[0] : null;
            var argValue = argSplit.Length > 1 ? argSplit[1] : null;

            if (!string.IsNullOrEmpty(argName))
                queryParams.Add(argName, argValue);
        }

        foreach (var p in parameters)
        {
            queryParams[p.Key] = p.Value switch
            {
                DateTime dateTime => dateTime.ToString(new CultureInfo("en-US")),
                DateOnly date => date.ToString(new CultureInfo("en-US")),
                _ => p.Value?.ToString()
            };
        }

        query = string.Join('&', queryParams.Where(x => x.Value != null).Select(kv => $"{kv.Key}={kv.Value}"));

        return $"{url}?{query}";
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

    public static string FormatPrice(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
        
        var cleanedInput = Regex.Replace(input, @"[^\d+-]", "");

        if (!long.TryParse(cleanedInput, out var number))
            return "Invalid input";
        
        var formattedNumber = number.ToString("#,##0", CultureInfo.InvariantCulture);

        return $"{formattedNumber} تومان";

    }
}