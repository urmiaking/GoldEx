using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Blogs.BlogCategories;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.BlogCategories.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class BlogCategoriesController(IBlogCategoryService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.BlogCategories.GetList)]
    public async Task<IActionResult> GetListAsync(CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.BlogCategories.Get)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> GetAsync([FromRoute] Guid id,  CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost(ApiRoutes.BlogCategories.Create)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> CreateAsync([FromBody] BlogCategoryRequest request, CancellationToken cancellationToken)
    {
        await service.CreateAsync(request, cancellationToken);
        return Created();
    }

    [HttpPut(ApiRoutes.BlogCategories.Update)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] BlogCategoryRequest request, CancellationToken cancellationToken)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPatch(ApiRoutes.BlogCategories.SetStatus)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> SetStatusAsync([FromRoute] Guid id, [FromRoute] bool isActive, CancellationToken cancellationToken)
    {
        await service.SetStatusAsync(id, isActive, cancellationToken);
        return NoContent();
    }

    [HttpDelete(ApiRoutes.BlogCategories.Delete)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}