using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.PriceUnits.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class PriceUnitsController(IPriceUnitService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.PriceUnits.GetList)]
    public async Task<IActionResult> GetListAsync(CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.PriceUnits.GetAll)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await service.GetAllAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.PriceUnits.GetTitles)]
    public async Task<IActionResult> GetTitlesAsync(CancellationToken cancellationToken = default)
    {
        var list = await service.GetTitlesAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.PriceUnits.Get)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var priceUnit = await service.GetAsync(id, cancellationToken);
        return Ok(priceUnit);
    }

    [HttpPost(ApiRoutes.PriceUnits.Create)]
    public async Task<IActionResult> CreateAsync(CreatePriceUnitRequest request, CancellationToken cancellationToken = default)
    {
        await service.CreateAsync(request, cancellationToken);
        return Created();
    }

    [HttpPut(ApiRoutes.PriceUnits.Update)]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdatePriceUnitRequest request, CancellationToken cancellationToken = default)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPut(ApiRoutes.PriceUnits.UpdateStatus)]
    public async Task<IActionResult> UpdateStatusAsync(Guid id, UpdatePriceUnitStatusRequest request, CancellationToken cancellationToken = default)
    {
        await service.UpdateStatusAsync(id, request, cancellationToken);
        return NoContent();
    }
}