using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.SmsLogs.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class SmsLogsController(ISmsLogService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.SmsLogs.GetList)]
    public async Task<IActionResult> GetListAsync([FromQuery] RequestFilter filter, CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(filter, cancellationToken);
        return Ok(list);
    }
}