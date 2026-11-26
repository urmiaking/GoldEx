using GoldEx.Sdk.Server.Api;
using GoldEx.Server.Common.Authorization;
using GoldEx.Shared.Constants;
using GoldEx.Shared.Services.Abstractions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Icons.Base)]
public class IconsController(IIconService iconService, IStoreContext storeContext) : ApiControllerBase
{
    //[AllowAnonymous]
    //[HttpGet(ApiRoutes.Icons.GetIcon)]
    //public async Task<IActionResult> GetIconAsync(IconType iconType, Guid id, CancellationToken cancellationToken = default)
    //{
    //    var icon = await iconService.GetIconAsync(iconType, id, cancellationToken);
    //    if (icon == null)
    //        return NotFound();

    //    return File(icon, "image/png");
    //}

    [AllowAnonymous]
    [HttpGet(ApiRoutes.Icons.GetIcon)]
    [DemoRead]
    public IActionResult GetIcon(
        IconType iconType,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (DemoAccess.IsDemo(User) && iconType == IconType.App)
        {
            if (storeContext.StoreId is not { } ownStore || (id != Guid.Empty && id != ownStore))
                return NotFound();
            id = ownStore;
        }
        var iconPath = iconService.GetIconPath(iconType, id);

        if (iconPath == null || !System.IO.File.Exists(iconPath))
            return NotFound();

        var fileInfo = new FileInfo(iconPath);

        var etag = $"\"{fileInfo.Length}-{fileInfo.LastWriteTimeUtc.Ticks}\"";

        if (Request.Headers.IfNoneMatch == etag)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        Response.Headers.ETag = etag;
        Response.Headers.CacheControl = "public,max-age=31536000";
        Response.Headers.LastModified = fileInfo.LastWriteTimeUtc.ToString("R");

        return PhysicalFile(iconPath, "image/png");
    }
}