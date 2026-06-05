using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.CheckPayments;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.CheckPayments.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class CheckPaymentsController(ICheckPaymentService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.CheckPayments.GetList)]
    public async Task<IActionResult> GetListAsync(
        [FromQuery] RequestFilter filter,
        [FromQuery] CheckPaymentFilter checkPaymentFilter,
        CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(filter, checkPaymentFilter, cancellationToken);
        return Ok(list);
    }

    [HttpPost(ApiRoutes.CheckPayments.Accept)]
    public async Task<IActionResult> AcceptAsync(
        [FromRoute] Guid id,
        [FromBody] AcceptCheckPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        await service.AcceptAsync(id, request, cancellationToken);
        return Ok();
    }

    [HttpPost(ApiRoutes.CheckPayments.Return)]
    public async Task<IActionResult> ReturnAsync(
        [FromRoute] Guid id,
        [FromBody] ReturnCheckPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        await service.ReturnAsync(id, request, cancellationToken);
        return Ok();
    }
}
