using Microsoft.AspNetCore.Hosting;

namespace GoldEx.Server.Application.Utilities;

public static class WebHostEnvironmentExtensions
{
    extension(IWebHostEnvironment environment)
    {
        public bool PriceHistoryIconExists(Guid id)
            => File.Exists(Path.Combine(environment.ContentRootPath, "uploads", "icons", "price-histories", $"{id}.png"));

        public string GetPriceHistoryIconPath(Guid id, string? contentType)
            => Path.Combine(environment.ContentRootPath, "uploads", "icons", "price-histories", $"{id}.{(string.IsNullOrEmpty(contentType) ? "png" : contentType)}");

        public bool PriceUnitIconExists(Guid id)
            => File.Exists(Path.Combine(environment.ContentRootPath, "uploads", "icons", "price-units", $"{id}.png"));

        public string GetPriceUnitIconPath(Guid id, string? contentType)
            => Path.Combine(environment.ContentRootPath, "uploads", "icons", "price-units", $"{id}.{(string.IsNullOrEmpty(contentType) ? "png" : contentType)}");

        public bool AppIconExists(string? storeSlug = null)
            => File.Exists(environment.GetAppIconPath(storeSlug));

        public string GetAppIconPath(string? storeSlug = null)
            => !string.IsNullOrWhiteSpace(storeSlug)
                ? Path.Combine(environment.ContentRootPath, "uploads", "icons", "app", $"logo_{storeSlug}.png")
                : Path.Combine(environment.ContentRootPath, "uploads", "icons", "app", "logo.png");

        public string GetAppIconDirectory()
            => Path.Combine(environment.ContentRootPath, "uploads", "icons", "app");

        public string GetInventoryEntryTemplateFilePath()
            => Path.Combine(environment.WebRootPath, "templates", "inventory-entry-template.xlsx");

        public string GetBlogPostDirectoryPath(Guid blogId)
            => Path.Combine(environment.ContentRootPath, "shared", "content", "blogs", blogId.ToString());

        public string GetBlogsTempDirectoryPath()
            => Path.Combine(environment.ContentRootPath, "shared", "content", "blogs", "temp");

        public string GetBlogPostFilePath(Guid blogId, string fileName)
            => Path.Combine(environment.ContentRootPath, "shared", "content", "blogs", blogId.ToString(), fileName);

        // URL / RELATIVE PATHS: Keep "uploads"
        // Used for saving src="..." in DB and returning JSON to Frontend
        public string GetBlogsTempDirectoryRelativePath(string fileName)
        {
            // Must match the RequestPath in Rule 1 of Middleware
            return Path.Combine("uploads", "content", "blogs", "temp", fileName).Replace("\\", "/");
        }

        public string GetCheckPaymentFilePath(Guid id, string? fileExtension = null)
        {
            var basePath = Path.Combine(environment.ContentRootPath, "uploads", "check-payments", id.ToString());
            return string.IsNullOrEmpty(fileExtension) ? basePath : $"{basePath}.{fileExtension}";
        }

        public string GetCheckPaymentDirectoryPath() => Path.Combine(environment.ContentRootPath, "uploads", "check-payments");

        public string GetReportsDirectory() => Path.Combine(environment.ContentRootPath, "Reports");

        public string GetStoreReportPath(string reportName, string storeSlug) => Path.Combine(environment.GetReportsDirectory(), $"{reportName}_{storeSlug}.repx");

        public string GetReportPath(string reportName) => Path.Combine(environment.GetReportsDirectory(), $"{reportName}.repx");
    }
}