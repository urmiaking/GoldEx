using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.ProductCategories.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class ProductCategoriesController(IProductCategoryService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.ProductCategories.GetList)]
    public async Task<IActionResult> GetListAsync(CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.ProductCategories.Get)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost(ApiRoutes.ProductCategories.Create)]
    public async Task<IActionResult> CreateAsync(CreateProductCategoryRequest request, CancellationToken cancellationToken)
    {
        await service.CreateAsync(request, cancellationToken);
        return Created();
    }

    [HttpPut(ApiRoutes.ProductCategories.Update)]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateProductCategoryRequest request, CancellationToken cancellationToken)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return Ok();
    }

    [HttpDelete(ApiRoutes.ProductCategories.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Ok();
    }
}