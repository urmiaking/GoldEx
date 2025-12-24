using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.CoinInstances.Base)]
public class CoinInstancesController(ICoinInstanceService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.CoinInstances.Get)]
    public async Task<IActionResult> GetAsync([FromRoute] string barcode, CancellationToken cancellationToken = default)
    {
        var result = await service.GetAsync(barcode, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }
}