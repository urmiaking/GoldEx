using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Enums;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace GoldEx.Shared.Helpers;

public static class PriceCatalogHelper
{
    private static readonly ConcurrentDictionary<string, PriceCatalog> Lookup;

    static PriceCatalogHelper()
    {
        Lookup = new ConcurrentDictionary<string, PriceCatalog>(
            Enum.GetValues<PriceCatalog>()
                .Select(value =>
                {
                    var display = value.GetAttribute<DisplayAttribute>()?.Name?.Trim();
                    var normalized = Normalize(display ?? value.ToString());
                    return new KeyValuePair<string, PriceCatalog>(normalized, value);
                })
                .ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase)
        );
    }

    public static bool TryGetByTitle(string title, out PriceCatalog priceCatalog)
    {
        title = Normalize(title);
        return Lookup.TryGetValue(title, out priceCatalog);
    }

    public static PriceCatalog? GetByTitle(string title)
    {
        return TryGetByTitle(title, out var result) ? result : null;
    }

    private static string Normalize(string input)
    {
        return input.Trim().ToPersianChars().ToEnglishNumericsChars();
    }

    private static T? GetAttribute<T>(this Enum value) where T : Attribute
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        return member?.GetCustomAttribute<T>();
    }
}