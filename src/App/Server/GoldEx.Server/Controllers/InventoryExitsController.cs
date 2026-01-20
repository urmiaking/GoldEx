using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.InventoryExits;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.InventoryExits.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class InventoryExitsController(IInventoryExitService service) : Controller
{
    [HttpGet(ApiRoutes.InventoryExits.GetList)]
    public async Task<IActionResult> GetListAsync([FromQuery] RequestFilter filter, CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(filter, cancellationToken);
        return Ok(list);
    }

    [HttpPost(ApiRoutes.InventoryExits.Exit)]
    public async Task<IActionResult> ExitAsync([FromBody] CreateInventoryExitRequest request, CancellationToken cancellationToken = default)
    {
        await service.ExitAsync(request, cancellationToken);
        return NoContent();
    }
}