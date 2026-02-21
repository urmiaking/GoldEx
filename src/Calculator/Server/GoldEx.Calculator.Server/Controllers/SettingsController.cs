using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Calculator.Server.Controllers;

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
}