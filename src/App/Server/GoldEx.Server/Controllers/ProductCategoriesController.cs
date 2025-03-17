using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Categories;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.ProductCategories.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class ProductCategoriesController(IProductCategoryClientService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.ProductCategories.GetAll)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var list = await service.GetAllAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.ProductCategories.Get)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost(ApiRoutes.ProductCategories.Create)]
    public async Task<IActionResult> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        await service.CreateAsync(request, cancellationToken);
        return Ok();
    }

    [HttpPut(ApiRoutes.ProductCategories.Update)]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return Ok();
    }

    [HttpDelete(ApiRoutes.ProductCategories.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.DeleteAsync(id, false, cancellationToken);
        return Ok();
    }

    [HttpGet(ApiRoutes.ProductCategories.GetPendingItems)]
    public async Task<IActionResult> GetPendingsAsync(DateTime checkPointDate, CancellationToken cancellationToken)
    {
        var list = await service.GetPendingsAsync(checkPointDate, cancellationToken);

        return Ok(list);
    }
}