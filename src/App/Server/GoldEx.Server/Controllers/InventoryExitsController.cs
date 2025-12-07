using GoldEx.Sdk.Common;
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
    [HttpPost(ApiRoutes.InventoryExits.Exit)]
    public async Task<IActionResult> ExitAsync([FromBody] CreateInventoryExitRequest request, CancellationToken cancellationToken = default)
    {
        await service.ExitAsync(request, cancellationToken);
        return NoContent();
    }
}