using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[AllowAnonymous]
[Route(ApiRoutes.Price.Base)]
public class PriceController(IPriceService priceService) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Price.Get)]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken = default)
    {
        var list = await priceService.GetAsync(cancellationToken);

        return Ok(list);
    }

    [HttpGet(ApiRoutes.Price.GetMarket)]
    public async Task<IActionResult> GetAsync(MarketType marketType, CancellationToken cancellationToken = default)
    {
        var list = await priceService.GetAsync(marketType, cancellationToken);

        return Ok(list);
    }

    [HttpGet(ApiRoutes.Price.GetUnit)]
    public async Task<IActionResult> GetAsync(UnitType unitType, CancellationToken cancellationToken = default)
    {
        var price = await priceService.GetAsync(unitType, cancellationToken);
        return price is not null ? Ok(price) : NotFound();
    }

    [HttpGet(ApiRoutes.Price.GetSettings)]
    public async Task<IActionResult> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await priceService.GetSettingsAsync(cancellationToken);
        return Ok(settings);
    }

    [HttpPut(ApiRoutes.Price.UpdateStatus)]
    public async Task<IActionResult> UpdateStatusAsync(Guid id, UpdatePriceStatusRequest request, CancellationToken cancellationToken = default)
    {
        await priceService.SetStatusAsync(id, request, cancellationToken);
        return NoContent();
    }
}