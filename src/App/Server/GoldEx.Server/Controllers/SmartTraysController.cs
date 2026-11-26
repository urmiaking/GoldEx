using GoldEx.Sdk.Common;
using GoldEx.Server.Common.Authorization;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.SmartTrays;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.SmartTrays.Base)]
[Authorize]
public class SmartTraysController(ISmartTrayService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.SmartTrays.GetActive)]
    [DemoRead]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}, Demo")]
    public async Task<IActionResult> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var trays = await service.GetActiveTraysAsync(cancellationToken);
        return Ok(trays);
    }

    [HttpGet(ApiRoutes.SmartTrays.GetById)]
    [DemoRead]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}, Demo")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tray = await service.GetTrayByIdAsync(id, cancellationToken);
        return Ok(tray);
    }

    [HttpPost(ApiRoutes.SmartTrays.Create)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateSmartTrayRequest request, CancellationToken cancellationToken = default)
    {
        var tray = await service.CreateTrayAsync(request, cancellationToken);
        return Ok(tray);
    }

    [HttpPost(ApiRoutes.SmartTrays.AddItem)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> AddItemAsync(Guid trayId, [FromBody] AddSmartTrayItemRequest request, CancellationToken cancellationToken = default)
    {
        var item = await service.AddItemByBarcodeAsync(trayId, request.Barcode, cancellationToken);
        return Ok(item);
    }

    [HttpPost(ApiRoutes.SmartTrays.ReturnItem)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> ReturnItemAsync(Guid trayId, string barcode, CancellationToken cancellationToken = default)
    {
        var item = await service.ReturnItemByBarcodeAsync(trayId, barcode, cancellationToken);
        return Ok(item);
    }

    [HttpDelete(ApiRoutes.SmartTrays.RemoveItem)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> RemoveItemAsync(Guid trayId, string barcode, CancellationToken cancellationToken = default)
    {
        await service.RemoveItemAsync(trayId, barcode, cancellationToken);
        return Ok();
    }

    [HttpPost(ApiRoutes.SmartTrays.MarkSold)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> MarkSoldAsync(Guid trayId, string barcode, Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var item = await service.MarkItemSoldAsync(trayId, barcode, invoiceId, cancellationToken);
        return Ok(item);
    }

    [HttpPost(ApiRoutes.SmartTrays.Close)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> CloseAsync(Guid trayId, CancellationToken cancellationToken = default)
    {
        await service.CloseTrayAsync(trayId, cancellationToken);
        return Ok();
    }

    [HttpPost(ApiRoutes.SmartTrays.Discrepancy)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> DiscrepancyAsync(Guid trayId, [FromBody] ReportSmartTrayDiscrepancyRequest request, CancellationToken cancellationToken = default)
    {
        await service.ReportDiscrepancyAsync(trayId, request, cancellationToken);
        return Ok();
    }

    [HttpPost(ApiRoutes.SmartTrays.Cancel)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> CancelAsync(Guid trayId, CancellationToken cancellationToken = default)
    {
        await service.CancelTrayAsync(trayId, cancellationToken);
        return Ok();
    }
}