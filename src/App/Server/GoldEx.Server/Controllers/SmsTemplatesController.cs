using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.SmsTemplates;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.SmsTemplates.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class SmsTemplatesController(ISmsTemplateService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.SmsTemplates.GetList)]
    public async Task<IActionResult> GetListAsync(CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpPut(ApiRoutes.SmsTemplates.Update)]
    public async Task<IActionResult> UpdateAsync(List<SmsTemplateRequest> requests,
        CancellationToken cancellationToken = default)
    {
        await service.UpdateAsync(requests, cancellationToken);
        return NoContent();
    }
}