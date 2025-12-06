using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.InventoryEntries;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.InventoryEntries.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class InventoryEntriesController(IInventoryEntryService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.InventoryEntries.GetList)]
    public async Task<ActionResult<List<InventoryEntryResponse>>> GetListAsync([FromQuery] RequestFilter filter, CancellationToken cancellationToken = default)
    {
        var result = await service.GetListAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpPost(ApiRoutes.InventoryEntries.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateInventoryEntryRequest request, CancellationToken cancellationToken = default)
    {
        await service.CreateAsync(request);
        return NoContent();
    }

    [RequestSizeLimit(15_000_000)]
    [HttpPost(ApiRoutes.InventoryEntries.ProcessExcel)]
    public async Task<ActionResult<List<GetProductItemEntryResponse>>> ProcessExcelAsync([FromBody] ProcessExcelRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.ProcessExcelAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete(ApiRoutes.InventoryEntries.Rollback)]
    public async Task<IActionResult> RollbackAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        await service.RollbackAsync(id, cancellationToken);
        return NoContent();
    }
}