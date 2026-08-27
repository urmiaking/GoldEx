using System;

namespace GoldEx.Shared.Helpers;

public static class VitrineUrlHelper
{
    /// <summary>
    /// Builds the public Vitrine URL for a specific product item.
    /// If customDomain is specified (e.g. fanijewellery.ir), it formats as:
    /// https://fanijewellery.ir/{storeSlug}/p/{barcode}
    /// Otherwise, it falls back to the current server base URL.
    /// </summary>
    public static string BuildProductVitrineUrl(string? customDomain, string currentBaseUri, string storeSlug, string barcode)
    {
        var baseDomain = FormatBaseDomain(customDomain, currentBaseUri);
        var cleanSlug = string.IsNullOrWhiteSpace(storeSlug) ? "default" : storeSlug.Trim();
        var cleanBarcode = barcode?.Trim() ?? string.Empty;

        return $"{baseDomain}/{cleanSlug}/p/{cleanBarcode}";
    }

    /// <summary>
    /// Builds the public Vitrine home page URL for a store.
    /// </summary>
    public static string BuildVitrineHomeUrl(string? customDomain, string currentBaseUri, string storeSlug)
    {
        var baseDomain = FormatBaseDomain(customDomain, currentBaseUri);
        var cleanSlug = string.IsNullOrWhiteSpace(storeSlug) ? "default" : storeSlug.Trim();

        return $"{baseDomain}/{cleanSlug}";
    }

    /// <summary>
    /// Builds the public Vitrine catalog URL for a store.
    /// </summary>
    public static string BuildVitrineCatalogUrl(string? customDomain, string currentBaseUri, string storeSlug)
    {
        var baseDomain = FormatBaseDomain(customDomain, currentBaseUri);
        var cleanSlug = string.IsNullOrWhiteSpace(storeSlug) ? "default" : storeSlug.Trim();

        return $"{baseDomain}/{cleanSlug}/catalog";
    }

    private static string FormatBaseDomain(string? customDomain, string currentBaseUri)
    {
        if (!string.IsNullOrWhiteSpace(customDomain))
        {
            var domain = customDomain.Trim().TrimEnd('/');
            if (!domain.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !domain.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                domain = $"https://{domain}";
            }
            return domain;
        }

        return currentBaseUri.TrimEnd('/');
    }
}
