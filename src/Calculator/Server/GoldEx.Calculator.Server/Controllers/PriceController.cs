using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Api;
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
}