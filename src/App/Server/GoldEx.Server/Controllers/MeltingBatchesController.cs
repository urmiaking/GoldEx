using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.MeltingBatches;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.MeltingBatches.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class MeltingBatchesController(IMeltingBatchService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.MeltingBatches.GetList)]
    public async Task<ActionResult> GetListAsync([FromQuery] RequestFilter requestFilter, [FromQuery] MeltingBatchFilter filter,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetListAsync(requestFilter, filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.MeltingBatches.Get)]
    public async Task<ActionResult> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await service.GetAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost(ApiRoutes.MeltingBatches.Create)]
    public async Task<ActionResult> CreateAsync([FromBody] MeltingBatchRequestDto request, CancellationToken cancellationToken = default)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut(ApiRoutes.MeltingBatches.SendToLab)]
    public async Task<ActionResult> SendToLabAsync(Guid id, [FromBody] SendToLabRequestDto request, CancellationToken cancellationToken = default)
    {
        await service.SendToLabAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPut(ApiRoutes.MeltingBatches.CompleteMelting)]
    public async Task<ActionResult> CompleteMeltingAsync(Guid id, [FromBody] CompleteMeltingRequestDto request, CancellationToken cancellationToken = default)
    {
        await service.CompleteMeltingAsync(id, request, cancellationToken);
        return NoContent();
    }
}