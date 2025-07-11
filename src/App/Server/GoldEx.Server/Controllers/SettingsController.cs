using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Settings.Base)]
public class SettingsController(ISettingService service) : ApiControllerBase
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
}