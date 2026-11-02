using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using GoldEx.Shared.DTOs.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Notifications.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class NotificationsController(INotificationService service, IPushNotificationService push) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Notifications.PushConfiguration)]
    public async Task<IActionResult> PushConfigurationAsync(CancellationToken ct) => Ok(await push.GetConfigurationAsync(ct));

    [HttpPost(ApiRoutes.Notifications.PushSubscribe)]
    public async Task<IActionResult> SubscribeAsync([FromBody] RegisterPushRequest request, CancellationToken ct)
    {
        await push.RegisterAsync(request, ct); return NoContent();
    }

    [HttpPost(ApiRoutes.Notifications.PushRevoke)]
    public async Task<IActionResult> RevokeAsync([FromBody] RevokePushRequest request, CancellationToken ct)
    {
        await push.RevokeAsync(request, ct); return NoContent();
    }

    [HttpPost(ApiRoutes.Notifications.PushTest)]
    public async Task<IActionResult> TestPushAsync([FromBody] RevokePushRequest request, CancellationToken ct) =>
        Ok(await push.SendTestAsync(request, ct));

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
