using GoldEx.Sdk.Common;
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
        var list = await priceService.GetListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpGet(ApiRoutes.Price.GetTitles)]
    public async Task<IActionResult> GetTitlesAsync([FromQuery] MarketType[] marketTypes, CancellationToken cancellationToken = default)
    {
        var list = await priceService.GetTitlesAsync(marketTypes, cancellationToken);

        return Ok(list);
    }

    [HttpGet(ApiRoutes.Price.GetMarket)]
    public async Task<IActionResult> GetAsync(MarketType marketType, CancellationToken cancellationToken = default)
    {
        var list = await priceService.GetListAsync(marketType, cancellationToken);

        return Ok(list);
    }

    [HttpGet(ApiRoutes.Price.GetUnit)]
    public async Task<IActionResult> GetAsync(UnitType unitType, Guid? priceUnitId, [FromQuery] bool applySafetyMargin, CancellationToken cancellationToken = default)
    {
        var price = await priceService.GetAsync(unitType, priceUnitId, applySafetyMargin, cancellationToken);
        return price is not null ? Ok(price) : NotFound();
    }

    [HttpGet(ApiRoutes.Price.GetExchange)]
    public async Task<IActionResult> GetExchangeRateAsync(Guid primaryPriceUnitId, Guid secondaryPriceUnitId, CancellationToken cancellationToken = default)
    {
        var exchangeRate = await priceService.GetExchangeRateAsync(primaryPriceUnitId, secondaryPriceUnitId, cancellationToken);
        return Ok(exchangeRate);
    }

    [HttpGet(ApiRoutes.Price.GetByPriceUnit)]
    public async Task<IActionResult> GetByPriceUnitAsync(Guid priceUnitId, CancellationToken cancellationToken = default)
    {
        var price = await priceService.GetAsync(priceUnitId, cancellationToken);
        return price is not null ? Ok(price) : NotFound();
    }

    [HttpGet(ApiRoutes.Price.GetSettings)]
    public async Task<IActionResult> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await priceService.GetSettingsAsync(cancellationToken);
        return Ok(settings);
    }

    [HttpPut(ApiRoutes.Price.UpdateStatus)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> UpdateStatusAsync(Guid id, UpdatePriceStatusRequest request, CancellationToken cancellationToken = default)
    {
        await priceService.SetStatusAsync(id, request, cancellationToken);
        return NoContent();
    }
}