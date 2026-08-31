using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.ProductAttributes;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.ProductAttributes.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class ProductAttributesController(IProductAttributeService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.ProductAttributes.GetList)]
    public async Task<IActionResult> GetListAsync(CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.ProductAttributes.Get)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost(ApiRoutes.ProductAttributes.Create)]
    public async Task<IActionResult> CreateAsync(CreateProductAttributeRequest request, CancellationToken cancellationToken)
    {
        await service.CreateAsync(request, cancellationToken);
        return Created();
    }

    [HttpPut(ApiRoutes.ProductAttributes.Update)]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateProductAttributeRequest request, CancellationToken cancellationToken)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return Ok();
    }

    [HttpDelete(ApiRoutes.ProductAttributes.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Ok();
    }

    [HttpGet(ApiRoutes.ProductAttributes.GetCategoryAttributes)]
    public async Task<IActionResult> GetCategoryAttributesAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var list = await service.GetCategoryAttributesAsync(categoryId, cancellationToken);
        return Ok(list);
    }

    [HttpPost(ApiRoutes.ProductAttributes.SetCategoryAttributes)]
    public async Task<IActionResult> SetCategoryAttributesAsync(SetCategoryAttributesRequest request, CancellationToken cancellationToken)
    {
        await service.SetCategoryAttributesAsync(request, cancellationToken);
        return Ok();
    }
}
