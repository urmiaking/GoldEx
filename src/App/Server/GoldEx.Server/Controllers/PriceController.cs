﻿using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Price.Base)]
public class PriceController(IPriceClientService priceService) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Price.GetLatestPrices)]
    public async Task<IActionResult> GetLatestPrices(CancellationToken cancellationToken = default)
    {
        var list = await priceService.GetLatestPricesAsync(cancellationToken);

        return Ok(list);
    }
}