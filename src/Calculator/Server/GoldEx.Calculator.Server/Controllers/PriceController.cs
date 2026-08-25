using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Calculator.Server.Controllers;

[AllowAnonymous]
[Route(ApiRoutes.Price.Base)]
public class PriceController(IPriceService priceService) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Price.Get)]
    public async Task<IActionResult> GetAsync([FromQuery] bool? isPinned = null, CancellationToken cancellationToken = default)
    {
        var list = await priceService.GetListAsync(isPinned, cancellationToken);
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
    public async Task<IActionResult> GetAsync(GoldUnitType unitType, Guid? priceUnitId, [FromQuery] bool applySafetyMargin, CancellationToken cancellationToken = default)
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

    [HttpGet(ApiRoutes.Price.GetByCatalog)]
    public async Task<IActionResult> GetByCatalogAsync(PriceCatalog priceCatalog, CancellationToken cancellationToken = default)
    {
        var price = await priceService.GetAsync(priceCatalog, cancellationToken);
        return price is not null ? Ok(price) : NotFound();
    }
}