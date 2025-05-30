using Microsoft.AspNetCore.Hosting;

namespace GoldEx.Server.Application.Utilities;

public static class WebHostEnvironmentExtensions
{
    public static bool PriceHistoryIconExists(this IWebHostEnvironment environment, Guid id) 
        => File.Exists(Path.Combine(environment.ContentRootPath, "uploads", "icons", "price-histories", $"{id}.png"));

    public static string GetPriceHistoryIconPath(this IWebHostEnvironment environment, Guid id, string? contentType) 
        => Path.Combine(environment.ContentRootPath, "uploads", "icons", "price-histories", $"{id}.{(string.IsNullOrEmpty(contentType) ? "png" : contentType)}");
}