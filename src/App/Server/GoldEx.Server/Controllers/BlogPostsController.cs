using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Server.Application.Utilities;
using GoldEx.Shared.DTOs.Blogs.BlogPosts;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.BlogPosts.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class BlogPostsController(
    IBlogPostService service,
    IWebHostEnvironment hostEnvironment) : ApiControllerBase
{
    [HttpGet(ApiRoutes.BlogPosts.Get)]
    public async Task<IActionResult> GetAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.BlogPosts.GetSlug)]
    public async Task<IActionResult> GetBySlugAsync([FromRoute] string slug, CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(slug, cancellationToken);
        return Ok(result);
    }

    [HttpPost(ApiRoutes.BlogPosts.Create)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> CreateAsync([FromBody] BlogPostRequest request, CancellationToken cancellationToken)
    {
        await service.CreateAsync(request, cancellationToken);
        return Created();
    }

    [HttpPut(ApiRoutes.BlogPosts.Update)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] BlogPostRequest request, CancellationToken cancellationToken)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPatch(ApiRoutes.BlogPosts.SetStatus)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> SetStatusAsync([FromRoute] Guid id, [FromRoute] bool isActive, CancellationToken cancellationToken)
    {
        await service.SetStatusAsync(id, isActive, cancellationToken);
        return NoContent();
    }

    [HttpDelete(ApiRoutes.BlogPosts.Delete)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [IgnoreAntiforgeryToken]
    [HttpPost(ApiRoutes.BlogPosts.UploadFiles)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> UploadFilesAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length is 0)
            return BadRequest("File is empty.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".webm" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return BadRequest("Invalid file type.");

        // 1. Generate Names and Paths
        var fileName = Path.GetRandomFileName() + extension;

        // PHYSICAL PATH: /app/shared/content/blogs/temp/xy82.jpg
        // We use this to Write to disk
        var physicalDir = hostEnvironment.GetBlogsTempDirectoryPath();
        var physicalPath = Path.Combine(physicalDir, fileName);

        // VIRTUAL URL: uploads/content/blogs/temp/xy82.jpg
        // We use this for the Browser
        var virtualUrl = hostEnvironment.GetBlogsTempDirectoryRelativePath(fileName).Replace('\\', '/');

        // 2. Ensure Shared Directory Exists
        if (!Directory.Exists(physicalDir))
            Directory.CreateDirectory(physicalDir);

        // 3. Write Directly to Shared Volume (No MemoryStream)
        try
        {
            await using var stream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write);
            await file.CopyToAsync(stream, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log this error
            return StatusCode(500, ex.Message);
        }

        // 4. Return the URL that maps to the shared folder via Middleware
        return Json(new { location = $"/{virtualUrl}" });
    }
}