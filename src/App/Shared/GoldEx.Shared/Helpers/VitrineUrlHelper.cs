using System;

namespace GoldEx.Shared.Helpers;

public static class VitrineUrlHelper
{
    /// <summary>
    /// Builds the public Vitrine URL for a specific product item.
    /// If customDomain is specified (e.g. fanijewellery.ir), it formats as:
    /// https://fanijewellery.ir/p/{barcode}
    /// Otherwise, it falls back to the multi-tenant format:
    /// {currentBaseUri}/{storeSlug}/p/{barcode}
    /// </summary>
    public static string BuildProductVitrineUrl(string? customDomain, string currentBaseUri, string storeSlug, string barcode)
    {
        var cleanBarcode = barcode?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(customDomain))
        {
            var baseDomain = FormatBaseDomain(customDomain, currentBaseUri);
            return $"{baseDomain}/p/{cleanBarcode}";
        }

        var cleanSlug = string.IsNullOrWhiteSpace(storeSlug) ? "default" : storeSlug.Trim();
        var baseUri = FormatBaseDomain(null, currentBaseUri);
        return $"{baseUri}/{cleanSlug}/p/{cleanBarcode}";
    }

    /// <summary>
    /// Builds the public Vitrine home page URL for a store.
    /// If customDomain is specified (e.g. fanijewellery.ir), it formats as:
    /// https://fanijewellery.ir
    /// Otherwise, it falls back to:
    /// {currentBaseUri}/{storeSlug}
    /// </summary>
    public static string BuildVitrineHomeUrl(string? customDomain, string currentBaseUri, string storeSlug)
    {
        if (!string.IsNullOrWhiteSpace(customDomain))
        {
            return FormatBaseDomain(customDomain, currentBaseUri);
        }

        var cleanSlug = string.IsNullOrWhiteSpace(storeSlug) ? "default" : storeSlug.Trim();
        var baseUri = FormatBaseDomain(null, currentBaseUri);
        return $"{baseUri}/{cleanSlug}";
    }

    /// <summary>
    /// Builds the public Vitrine catalog URL for a store.
    /// If customDomain is specified (e.g. fanijewellery.ir), it formats as:
    /// https://fanijewellery.ir/catalog
    /// Otherwise, it falls back to:
    /// {currentBaseUri}/{storeSlug}/catalog
    /// </summary>
    public static string BuildVitrineCatalogUrl(string? customDomain, string currentBaseUri, string storeSlug)
    {
        if (!string.IsNullOrWhiteSpace(customDomain))
        {
            var baseDomain = FormatBaseDomain(customDomain, currentBaseUri);
            return $"{baseDomain}/catalog";
        }

        var cleanSlug = string.IsNullOrWhiteSpace(storeSlug) ? "default" : storeSlug.Trim();
        var baseUri = FormatBaseDomain(null, currentBaseUri);
        return $"{baseUri}/{cleanSlug}/catalog";
    }

    /// <summary>
    /// Builds the public Vitrine about page URL for a store.
    /// </summary>
    public static string BuildVitrineAboutUrl(string? customDomain, string currentBaseUri, string storeSlug)
    {
        if (!string.IsNullOrWhiteSpace(customDomain))
        {
            var baseDomain = FormatBaseDomain(customDomain, currentBaseUri);
            return $"{baseDomain}/about";
        }

        var cleanSlug = string.IsNullOrWhiteSpace(storeSlug) ? "default" : storeSlug.Trim();
        var baseUri = FormatBaseDomain(null, currentBaseUri);
        return $"{baseUri}/{cleanSlug}/about";
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
