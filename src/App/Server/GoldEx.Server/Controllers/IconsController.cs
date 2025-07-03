using GoldEx.Sdk.Server.Api;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Icons.Base)]
public class IconsController(IIconService iconService) : ApiControllerBase
{
    [AllowAnonymous]
    [HttpGet(ApiRoutes.Icons.GetIcon)]
    public async Task<IActionResult> GetIconAsync(IconType iconType, Guid id, CancellationToken cancellationToken = default)
    {
        var icon = await iconService.GetIconAsync(iconType, id, cancellationToken);
        if (icon == null)
            return NotFound();

        return File(icon, "image/png");
    }
}