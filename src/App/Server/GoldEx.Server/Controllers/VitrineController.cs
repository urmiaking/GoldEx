using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.Constants;
using GoldEx.Shared.DTOs.Vitrine;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Vitrine.Base)]
[EnableRateLimiting(RateLimitPolicies.Vitrine)]
public class VitrineController(
    IVitrineService service,
    IWebHostEnvironment hostEnvironment) : ApiControllerBase
{
    [AllowAnonymous]
    [HttpGet(ApiRoutes.Vitrine.GetStoreInfo)]
    public async Task<IActionResult> GetStoreInfoAsync([FromRoute] string storeSlug, CancellationToken cancellationToken)
    {
        var info = await service.GetStoreInfoAsync(storeSlug, cancellationToken);
        return info is null ? NotFound() : Ok(info);
    }

    [AllowAnonymous]
    [HttpGet(ApiRoutes.Vitrine.GetProducts)]
    public async Task<IActionResult> GetProductsAsync(
        [FromRoute] string storeSlug,
        [FromQuery] Guid? categoryId,
        [FromQuery] bool? onlyFeatured,
        CancellationToken cancellationToken)
    {
        var products = await service.GetVitrineProductsAsync(storeSlug, categoryId, onlyFeatured, cancellationToken);
        return Ok(products);
    }

    [AllowAnonymous]
    [HttpGet(ApiRoutes.Vitrine.GetProductDetail)]
    public async Task<IActionResult> GetProductDetailAsync(
        [FromRoute] string storeSlug,
        [FromRoute] string barcode,
        CancellationToken cancellationToken)
    {
        var detail = await service.GetProductDetailAsync(storeSlug, barcode, cancellationToken);
        return detail is null ? NotFound() : Ok(detail);
    }

    [AllowAnonymous]
    [HttpGet(ApiRoutes.Vitrine.GetCategories)]
    public async Task<IActionResult> GetCategoriesAsync([FromRoute] string storeSlug, CancellationToken cancellationToken)
    {
        var categories = await service.GetCategoriesAsync(storeSlug, cancellationToken);
        return Ok(categories);
    }

    [HttpPut(ApiRoutes.Vitrine.UpdateProductVitrine)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> UpdateProductVitrineAsync(
        [FromRoute] Guid productId,
        [FromBody] UpdateProductVitrineRequest request,
        CancellationToken cancellationToken)
    {
        await service.UpdateProductVitrineAsync(productId, request, cancellationToken);
        return NoContent();
    }

    [IgnoreAntiforgeryToken]
    [HttpPost(ApiRoutes.Vitrine.UploadProductImage)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    [RequestSizeLimit(30_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 30_000_000)]
    public async Task<IActionResult> UploadProductImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("فایلی انتخاب نشده است.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return BadRequest("فرمت فایل مجاز نیست. لطفاً از فرمت‌های jpg, png یا webp استفاده کنید.");

        var uploadsDir = Path.Combine(hostEnvironment.ContentRootPath, "uploads", "products");
        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.CreateVersion7()}{extension}";
        var physicalPath = Path.Combine(uploadsDir, fileName);

        await using (var stream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var relativeUrl = $"/uploads/products/{fileName}";
        return Ok(new { url = relativeUrl });
    }
}
