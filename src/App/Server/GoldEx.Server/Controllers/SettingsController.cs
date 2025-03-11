using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Settings.Base)]
public class SettingsController(ISettingsClientService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Settings.GetAll)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var settings = await service.GetAsync(cancellationToken);

        return settings is null ? NotFound() : Ok(settings);
    }

    [HttpGet(ApiRoutes.Settings.Get)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken = default)
    {
        var settings = await service.GetAsync(id, cancellationToken);

        return settings is null ? NotFound() : Ok(settings);
    }

    [HttpPut(ApiRoutes.Settings.Update)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> Update(Guid id, UpdateSettingsRequest request, CancellationToken cancellationToken = default)
    {
        await service.UpdateAsync(id, request, cancellationToken);

        return Ok();
    }


    [HttpGet(ApiRoutes.Settings.GetUpdate)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> GetUpdate(DateTime checkPointDate, CancellationToken cancellationToken = default)
    {
        var update = await service.GetUpdateAsync(checkPointDate, cancellationToken);

        return update is null ? NoContent() : Ok(update);
    }
}