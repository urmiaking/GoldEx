using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.PersonalAccessTokens;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.PersonalAccessTokens.Base)]
[Authorize]
public class PersonalAccessTokensController(IPersonalAccessTokenService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.PersonalAccessTokens.GetList)]
    public async Task<IActionResult> GetListAsync(CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpPost(ApiRoutes.PersonalAccessTokens.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreatePersonalAccessTokenRequest request, CancellationToken cancellationToken)
    {
        var response = await service.CreateAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPut(ApiRoutes.PersonalAccessTokens.Revoke)]
    public async Task<IActionResult> RevokeAsync(Guid id, CancellationToken cancellationToken)
    {
        await service.RevokeAsync(id, cancellationToken);
        return Ok();
    }

    [HttpDelete(ApiRoutes.PersonalAccessTokens.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Ok();
    }
}
