using Microsoft.AspNetCore.Hosting;

namespace GoldEx.Server.Application.Utilities;

public static class WebHostEnvironmentExtensions
{
    public static bool PriceHistoryIconExists(this IWebHostEnvironment environment, Guid id) 
        => File.Exists(Path.Combine(environment.ContentRootPath, "uploads", "icons", "price-histories", $"{id}.png"));

    public static string GetPriceHistoryIconPath(this IWebHostEnvironment environment, Guid id, string? contentType) 
        => Path.Combine(environment.ContentRootPath, "uploads", "icons", "price-histories", $"{id}.{(string.IsNullOrEmpty(contentType) ? "png" : contentType)}");

    public static bool PriceUnitIconExists(this IWebHostEnvironment environment, Guid id)
        => File.Exists(Path.Combine(environment.ContentRootPath, "uploads", "icons", "price-units", $"{id}.png"));

    public static string GetPriceUnitIconPath(this IWebHostEnvironment environment, Guid id, string? contentType)
        => Path.Combine(environment.ContentRootPath, "uploads", "icons", "price-units", $"{id}.{(string.IsNullOrEmpty(contentType) ? "png" : contentType)}");

    public static bool AppIconExists(this IWebHostEnvironment environment)
        => File.Exists(Path.Combine(environment.ContentRootPath, "uploads", "icons", "app", "logo.png"));

    public static string GetAppIconPath(this IWebHostEnvironment environment)
        => Path.Combine(environment.ContentRootPath, "uploads", "icons", "app", "logo.png");

    public static string GetAppIconDirectory(this IWebHostEnvironment environment)
        => Path.Combine(environment.ContentRootPath, "uploads", "icons", "app");

    public static string GetInventoryEntryTemplateFilePath(this IWebHostEnvironment environment)
        => Path.Combine(environment.WebRootPath, "templates", "inventory-entry-template.xlsx");

    public static string GetBlogPostDirectoryPath(this IWebHostEnvironment environment, Guid blogId)
        => Path.Combine(environment.ContentRootPath, "shared", "content", "blogs", blogId.ToString());

    public static string GetBlogsTempDirectoryPath(this IWebHostEnvironment environment)
        => Path.Combine(environment.ContentRootPath, "shared", "content", "blogs", "temp");

    public static string GetBlogPostFilePath(this IWebHostEnvironment environment, Guid blogId, string fileName)
        => Path.Combine(environment.ContentRootPath, "shared", "content", "blogs", blogId.ToString(), fileName);

    // URL / RELATIVE PATHS: Keep "uploads"
    // Used for saving src="..." in DB and returning JSON to Frontend

    public static string GetBlogsTempDirectoryRelativePath(this IWebHostEnvironment environment, string fileName)
    {
        // Must match the RequestPath in Rule 1 of Middleware
        return Path.Combine("uploads", "content", "blogs", "temp", fileName).Replace("\\", "/");
    }
}