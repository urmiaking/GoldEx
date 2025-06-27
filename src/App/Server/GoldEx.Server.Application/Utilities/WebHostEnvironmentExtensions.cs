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

}