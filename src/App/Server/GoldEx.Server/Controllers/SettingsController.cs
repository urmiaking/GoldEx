using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Settings.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class SettingsController(ISettingService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Settings.Get)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPut(ApiRoutes.Settings.Update)]
    public async Task<IActionResult> Update(UpdateSettingRequest request, CancellationToken cancellationToken = default)
    {
        await service.UpdateAsync(request, cancellationToken);
        return Ok();
    }
}