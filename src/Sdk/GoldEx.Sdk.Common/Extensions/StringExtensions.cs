using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GoldEx.Sdk.Common.Extensions;

public static class StringExtensions
{
    extension(string template)
    {
        public string FormatRoute() => template.FormatRoute(new { });

        public string FormatRoute(object parameters)
        {
            var route = template.RichFormat(parameters);
            route = route.Replace("//", "/");

            return route;
        }

        public string RichFormat(object parameters)
        {
            var properties = parameters.GetType().GetProperties();
            var dictionary = properties.ToDictionary(property => property.Name, property => property.GetValue(parameters));

            return template.RichFormat(dictionary);
        }

        public string RichFormat(Dictionary<string, object?> parameters)
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

        public string AppendQueryString(object? parameters)
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

        public string AppendQueryString(Dictionary<string, object?> parameters)
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

        public string ToPersianChars()
        {
            return string.IsNullOrEmpty(template) ? template : template.Replace('ك', 'ک').Replace('ي', 'ی');
        }

        public string ToEnglishNumericsChars()
        {
            if (string.IsNullOrEmpty(template))
                return template;

            return template.Replace('۰', '0')
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

        public IEnumerable<string> ExpandVariants()
        {
            yield return template;                         // نیم ست
            yield return template.Replace(" ", "");        // نیمست
            yield return template.Replace(" ", "\u200C");  // نیم‌ست
        }

        public string NormalizeText()
        {
            if (string.IsNullOrWhiteSpace(template))
                return string.Empty;

            var t = template.Trim();

            t = t.Replace("ي", "ی")
                .Replace("ى", "ی")
                .Replace("ئ", "ی")
                .Replace("ؤ", "و")
                .Replace("ك", "ک")
                .Replace("\u200C", " "); // remove ZWNJ

            while (t.Contains("  "))
                t = t.Replace("  ", " ");

            return t;
        }
    }

    public static string FormatDateString(this string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr))
            return dateStr ?? string.Empty;

        // Must be exactly yyyyMMdd and all digits
        if (dateStr.Length != 8 || !dateStr.All(char.IsDigit))
            return dateStr;

        // Validate it is a real date
        if (!DateTime.TryParseExact(
                dateStr,
                "yyyyMMdd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _))
            return dateStr;

        return $"{dateStr[..4]}/{dateStr.Substring(4, 2)}/{dateStr.Substring(6, 2)}";
    }


    extension(string name)
    {
        public string GetInitials()
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            // Handle both English and Arabic names correctly.
            var names = Regex.Split(name.Trim(), @"\s+"); // Split by any whitespace

            return names.Length == 0 ? "" : names.Where(n => !string.IsNullOrEmpty(n)).Aggregate("", (current, n) => current + n[..1]);
        }

        public string GetInitialsWithPeriods()
        {
            var initials = GetInitials(name);
            return string.Join(".", initials.ToCharArray()); // Add periods
        }
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

    public static string GenerateRandomPhoneNumber()
    {
        var randomNumber = IntExtensions.GenerateRandomNumber(100000000, 999999999);
        return "09" + randomNumber;
    }

    extension(string roleName)
    {
        public string? GetRoleName()
        {
            return roleName switch
            {
                BuiltinRoles.Administrators => "ادمین سایت",
                BuiltinRoles.Owners => "صاحب کسب و کار",
                BuiltinRoles.Customers => "مشتری",
                BuiltinRoles.Karats => "کاربر ماشین حساب",
                _ => null
            };
        }
    }
}