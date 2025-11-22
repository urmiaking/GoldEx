using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Infrastructure.Services.Abstractions;
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
    IFileService fileService,
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

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) &&
            !file.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only images and videos are allowed.");

        var safeName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);
        var path = Path.Combine(hostEnvironment.GetBlogsTempDirectoryRelativePath(safeName)).Replace('\\', '/');

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, cancellationToken);
        await fileService.SaveLocalFileAsync(path, ms.ToArray(), cancellationToken);

        return Json(new { location = $"/{path}" });
    }
}