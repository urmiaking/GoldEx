using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Products.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class ProductsController(IProductService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Products.GetList)]
    public async Task<IActionResult> GetListAsync([FromQuery] RequestFilter filter, CancellationToken cancellationToken)
    {
        var list = await service.GetListAsync(filter, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.Products.Get)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await service.GetAsync(id, cancellationToken);
        return Ok(product);
    }

    [HttpGet(ApiRoutes.Products.GetByBarcode)]
    public async Task<IActionResult> GetAsync(string barcode, CancellationToken cancellationToken)
    {
        var product = await service.GetAsync(barcode, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost(ApiRoutes.Products.Create)]
    public async Task<IActionResult> CreateAsync(ProductRequestDto request, CancellationToken cancellationToken)
    {
        await service.CreateAsync(request, cancellationToken);
        return Created();
    }

    [HttpPut(ApiRoutes.Products.Update)]
    public async Task<IActionResult> UpdateAsync(Guid id, ProductRequestDto request, CancellationToken cancellationToken)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return Ok();
    }

    [HttpDelete(ApiRoutes.Products.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Ok();
    }
}