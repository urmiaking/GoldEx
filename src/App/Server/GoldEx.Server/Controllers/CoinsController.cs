using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Coins.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class CoinsController(ICoinService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Coins.GetList)]
    public async Task<IActionResult> GetListAsync([FromQuery] bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(isActive, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.Coins.Get)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpGet(ApiRoutes.Coins.GetPrice)]
    public async Task<IActionResult> GetPriceAsync(Guid coinId, Guid? priceUnitId,
        CancellationToken cancellationToken = default)
    {
        var price = await service.GetPriceAsync(coinId, priceUnitId, cancellationToken);
        return price is null ? NotFound() : Ok(price);
    }

    [HttpPost(ApiRoutes.Coins.Create)]
    public async Task<IActionResult> CreateAsync(CoinRequestDto request, CancellationToken cancellationToken = default)
    {
        await service.CreateAsync(request, cancellationToken);
        return Created();
    }

    [HttpPut(ApiRoutes.Coins.Update)]
    public async Task<IActionResult> UpdateAsync(Guid id, CoinRequestDto request, CancellationToken cancellationToken = default)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete(ApiRoutes.Coins.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPut(ApiRoutes.Coins.SetStatus)]
    public async Task<IActionResult> SetStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        await service.SetStatusAsync(id, isActive, cancellationToken);
        return NoContent();
    }
}