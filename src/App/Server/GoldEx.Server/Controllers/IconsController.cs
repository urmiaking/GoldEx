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
    public IActionResult GetIcon(IconType iconType, Guid id, CancellationToken cancellationToken = default)
    {
        var path = iconService.GetIconPath(iconType, id);
        if (path == null)
            return NotFound();

        return PhysicalFile(path, "image/png");
    }
}