using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Calculator.Server.Controllers;

[AllowAnonymous]
[Route(ApiRoutes.PriceUnits.Base)]
public class PriceUnitsController(IPriceUnitService service) : ApiControllerBase
{
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

    [HttpGet(ApiRoutes.PriceUnits.GetDefault)]
    public async Task<IActionResult> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        var item = await service.GetDefaultAsync(cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }
}