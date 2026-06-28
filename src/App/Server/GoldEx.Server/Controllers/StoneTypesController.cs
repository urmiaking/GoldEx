using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.StoneTypes;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.StoneTypes.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class StoneTypesController(IStoneTypeService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.StoneTypes.GetList)]
    public async Task<IActionResult> GetListAsync([FromQuery] StoneTypeRequestFilter filter, CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(filter, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.StoneTypes.Get)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost(ApiRoutes.StoneTypes.Create)]
    public async Task<IActionResult> CreateAsync(CreateStoneTypeRequest request, CancellationToken cancellationToken)
    {
        await service.CreateAsync(request, cancellationToken);
        return Created();
    }

    [HttpPut(ApiRoutes.StoneTypes.Update)]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateStoneTypeRequest request, CancellationToken cancellationToken)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return Ok();
    }

    [HttpPut(ApiRoutes.StoneTypes.ToggleStatus)]
    public async Task<IActionResult> ToggleStatusAsync(Guid id, CancellationToken cancellationToken)
    {
        await service.ToggleStatusAsync(id, cancellationToken);
        return Ok();
    }
}
