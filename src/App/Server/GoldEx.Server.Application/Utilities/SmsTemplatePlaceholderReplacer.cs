using System.Text.RegularExpressions;

namespace GoldEx.Server.Application.Utilities;

internal static class SmsTemplatePlaceholderReplacer
{
    /// <summary>
    /// Matches placeholders in the form:
    /// (Name) or (Name:Unit)
    /// </summary>
    private static readonly Regex PlaceholderRegex =
        new(@"\(([^:)]+)(?::([^)]+))?\)", RegexOptions.Compiled);

    public static string Replace(
        string? template,
        IReadOnlyCollection<PlaceholderValue> values)
    {
        if (string.IsNullOrWhiteSpace(template))
            return string.Empty;

        if (values.Count == 0)
            return template;

        return PlaceholderRegex.Replace(template, match =>
        {
            var placeholderName = match.Groups[1].Value;
            var unitRequested = match.Groups[2].Success;

            var normalizedPlaceholder = Normalize(placeholderName);

            var value = values.FirstOrDefault(v =>
                Normalize(v.Name) == normalizedPlaceholder);

            if (value is null)
                return match.Value;

            if (unitRequested && !string.IsNullOrWhiteSpace(value.Unit))
                return $"{value.Value}{value.Unit}";

            return value.Value;
        });
    }

    private static string Normalize(string value)
        => value.Replace(" ", "")
            .Replace("_", "")
            .Trim();
}

internal sealed record PlaceholderValue(
    string Name,
    string Value,
    string? Unit = null);