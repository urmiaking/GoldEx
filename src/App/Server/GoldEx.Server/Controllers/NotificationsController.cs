using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Notifications.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class NotificationsController(INotificationService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Notifications.GetList)]
    public async Task<IActionResult> GetListAsync([FromQuery] bool? isRead, CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(isRead, cancellationToken);
        return Ok(list);
    }

    [HttpPut(ApiRoutes.Notifications.MarkAllAsRead)]
    public async Task<IActionResult> MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        await service.MarkAllAsReadAsync(cancellationToken);
        return NoContent();
    }

    [HttpPut(ApiRoutes.Notifications.MarkAsRead)]
    public async Task<IActionResult> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.MarkAsReadAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpDelete(ApiRoutes.Notifications.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}