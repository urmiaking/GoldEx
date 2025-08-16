using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.InventoryStocks.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class InventoryStocksController(IInventoryStockService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.InventoryStocks.GetList)]
    public async Task<IActionResult> GetListAsync([FromQuery] RequestFilter filter, [FromQuery] InventoryFilter inventoryFilter,
        CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(filter, inventoryFilter, cancellationToken);
        return Ok(list);
    }
}