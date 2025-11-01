using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.DTOs.Settings.Barcodes;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Settings.Base)]
public class SettingsController(ISettingService service, IBarcodePrintSettingsService barcodePrintSettingsService) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Settings.Get)]
    [AllowAnonymous]
    public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(cancellationToken);
        return Ok(item);
    }

    [HttpPut(ApiRoutes.Settings.Update)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> Update(UpdateSettingRequest request, CancellationToken cancellationToken = default)
    {
        await service.UpdateAsync(request, cancellationToken);
        return Ok();
    }

    [HttpGet(ApiRoutes.Settings.GetBarcodePrintSettings)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> GetBarcodePrintSettingsAsync(CancellationToken cancellationToken = default)
    {
        var result = await barcodePrintSettingsService.GetAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPut(ApiRoutes.Settings.UpdateBarcodePrintSettings)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> UpdateBarcodePrintSettingsAsync(
        [FromBody] UpdateBarcodePrintSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        await barcodePrintSettingsService.UpdateAsync(request, cancellationToken);
        return NoContent();
    }
}