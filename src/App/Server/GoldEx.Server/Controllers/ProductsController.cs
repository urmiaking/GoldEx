using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Products.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class ProductsController(IProductService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Products.GetListByName)]
    public async Task<IActionResult> GetListAsync([FromRoute] ProductType productType, [FromQuery] string name, CancellationToken cancellationToken)
    {
        var list = await service.GetListAsync(name, productType, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.Products.GetByBarcode)]
    public async Task<IActionResult> GetAsync(string barcode, CancellationToken cancellationToken = default)
    {
        var product = await service.GetAsync(barcode, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPut(ApiRoutes.Products.Update)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] ProductRequestDto request, CancellationToken cancellationToken)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }
}