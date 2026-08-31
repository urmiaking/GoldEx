using System.Text;
using System.Xml.Linq;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Server.Infrastructure.Specifications.Stores;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Controllers;

[ApiController]
[AllowAnonymous]
public class VitrineSeoController(
    IStoreRepository storeRepository,
    IProductRepository productRepository) : ControllerBase
{
    private static readonly XNamespace SitemapNs = "http://www.sitemaps.org/schemas/sitemap/0.9";
    private static readonly XNamespace ImageNs = "http://www.google.com/schemas/sitemap-image/1.1";

    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetMasterSitemapAsync(CancellationToken cancellationToken)
    {
        var scheme = Request.Scheme;
        var host = Request.Host.Value;
        var baseUrl = $"{scheme}://{host}";

        var activeStores = await storeRepository.Get(new ActiveStoresSpecification())
            .AsNoTracking()
            .IgnoreQueryFilters()
            .ToListAsync(cancellationToken);

        var urlset = new XElement(SitemapNs + "urlset",
            new XAttribute(XNamespace.Xmlns + "image", ImageNs.NamespaceName));

        foreach (var store in activeStores)
        {
            var storeSlug = store.Slug;

            // 1. Store Home Page
            urlset.Add(CreateUrlElement(
                $"{baseUrl}/{storeSlug}",
                DateTime.UtcNow,
                "daily",
                "1.0",
                store.LogoUrl != null ? $"{baseUrl}{store.LogoUrl}" : null,
                $"ویترین طلا و جواهر {store.Name}"));

            // 2. Store Catalog Page
            urlset.Add(CreateUrlElement(
                $"{baseUrl}/{storeSlug}/catalog",
                DateTime.UtcNow,
                "daily",
                "0.9"));

            // 3. Store About Page
            urlset.Add(CreateUrlElement(
                $"{baseUrl}/{storeSlug}/about",
                DateTime.UtcNow,
                "weekly",
                "0.6"));

            // 4. Products in Vitrine
            var products = await productRepository.Get(new ProductsForVitrineSpecification(store.Id.Value))
                .AsNoTracking()
                .IgnoreQueryFilters()
                .ToListAsync(cancellationToken);

            foreach (var product in products)
            {
                var mainImage = product.Images.FirstOrDefault(img => img.IsMain)?.Url
                                ?? product.Images.FirstOrDefault()?.Url;

                var imgFullUrl = !string.IsNullOrWhiteSpace(mainImage)
                    ? (mainImage.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? mainImage : $"{baseUrl}{mainImage}")
                    : null;

                urlset.Add(CreateUrlElement(
                    $"{baseUrl}/{storeSlug}/p/{product.Barcode}",
                    product.CreatedAt,
                    "daily",
                    product.IsFeatured ? "0.9" : "0.8",
                    imgFullUrl,
                    $"{product.Name} عیار {product.Fineness:G29} - {store.Name}"));
            }
        }

        var xmlDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), urlset);
        return Content(xmlDoc.Declaration + Environment.NewLine + xmlDoc, "application/xml", Encoding.UTF8);
    }

    [HttpGet("/{storeSlug}/sitemap.xml")]
    [HttpGet("/sitemap-{storeSlug}.xml")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetStoreSitemapAsync([FromRoute] string storeSlug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(storeSlug))
            return NotFound();

        var normalizedSlug = storeSlug.ToLowerInvariant().Trim();
        var store = await storeRepository.Get(new StoreBySlugSpecification(normalizedSlug))
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.IsActive, cancellationToken);

        if (store == null)
            return NotFound();

        var scheme = Request.Scheme;
        var host = Request.Host.Value;
        var baseUrl = $"{scheme}://{host}";

        var urlset = new XElement(SitemapNs + "urlset",
            new XAttribute(XNamespace.Xmlns + "image", ImageNs.NamespaceName));

        // Home
        urlset.Add(CreateUrlElement(
            $"{baseUrl}/{store.Slug}",
            DateTime.UtcNow,
            "daily",
            "1.0",
            store.LogoUrl != null ? $"{baseUrl}{store.LogoUrl}" : null,
            $"ویترین طلا و جواهر {store.Name}"));

        // Catalog
        urlset.Add(CreateUrlElement(
            $"{baseUrl}/{store.Slug}/catalog",
            DateTime.UtcNow,
            "daily",
            "0.9"));

        // About
        urlset.Add(CreateUrlElement(
            $"{baseUrl}/{store.Slug}/about",
            DateTime.UtcNow,
            "weekly",
            "0.6"));

        // Products
        var products = await productRepository.Get(new ProductsForVitrineSpecification(store.Id.Value))
            .AsNoTracking()
            .IgnoreQueryFilters()
            .ToListAsync(cancellationToken);

        foreach (var product in products)
        {
            var mainImage = product.Images.FirstOrDefault(img => img.IsMain)?.Url
                            ?? product.Images.FirstOrDefault()?.Url;

            var imgFullUrl = !string.IsNullOrWhiteSpace(mainImage)
                ? (mainImage.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? mainImage : $"{baseUrl}{mainImage}")
                : null;

            urlset.Add(CreateUrlElement(
                $"{baseUrl}/{store.Slug}/p/{product.Barcode}",
                product.CreatedAt,
                "daily",
                product.IsFeatured ? "0.9" : "0.8",
                imgFullUrl,
                $"{product.Name} عیار {product.Fineness:G29} - {store.Name}"));
        }

        var xmlDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), urlset);
        return Content(xmlDoc.Declaration + Environment.NewLine + xmlDoc, "application/xml", Encoding.UTF8);
    }

    [HttpGet("/robots.txt")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public IActionResult GetRobotsTxt()
    {
        var scheme = Request.Scheme;
        var host = Request.Host.Value;
        var sitemapUrl = $"{scheme}://{host}/sitemap.xml";

        var sb = new StringBuilder();
        sb.AppendLine("User-agent: *");
        sb.AppendLine("Allow: /");
        sb.AppendLine("Disallow: /admin/");
        sb.AppendLine("Disallow: /api/");
        sb.AppendLine("Disallow: /mcp/");
        sb.AppendLine("Disallow: /oauth/");
        sb.AppendLine("Disallow: /base-info/");
        sb.AppendLine("Disallow: /invoices/");
        sb.AppendLine("Disallow: /inventory/");
        sb.AppendLine("Disallow: /accounting/");
        sb.AppendLine("Disallow: /finances/");
        sb.AppendLine("Disallow: /reports/");
        sb.AppendLine("Disallow: /settings/");
        sb.AppendLine("Disallow: /Account/");
        sb.AppendLine();
        sb.AppendLine($"Sitemap: {sitemapUrl}");

        return Content(sb.ToString(), "text/plain", Encoding.UTF8);
    }

    private static XElement CreateUrlElement(
        string loc,
        DateTime lastMod,
        string changeFreq,
        string priority,
        string? imageUrl = null,
        string? imageTitle = null)
    {
        var urlElement = new XElement(SitemapNs + "url",
            new XElement(SitemapNs + "loc", loc),
            new XElement(SitemapNs + "lastmod", lastMod.ToString("yyyy-MM-dd")),
            new XElement(SitemapNs + "changefreq", changeFreq),
            new XElement(SitemapNs + "priority", priority));

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            var imgElement = new XElement(ImageNs + "image",
                new XElement(ImageNs + "loc", imageUrl));

            if (!string.IsNullOrWhiteSpace(imageTitle))
            {
                imgElement.Add(new XElement(ImageNs + "title", imageTitle));
            }

            urlElement.Add(imgElement);
        }

        return urlElement;
    }
}
