using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.BarcodeReservations;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.BarcodeReservations.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public sealed class BarcodeReservationsController(IBarcodeReservationService service) : ApiControllerBase
{
    [HttpPost(ApiRoutes.BarcodeReservations.IssueNext)]
    [ProducesResponseType(typeof(IssueNextBarcodeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> IssueNextAsync(
        [FromBody] IssueNextBarcodeRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.IssueNextAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut(ApiRoutes.BarcodeReservations.Release)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReleaseAsync(
        [FromRoute] string barcode,
        CancellationToken cancellationToken = default)
    {
        await service.ReleaseAsync(barcode, cancellationToken);
        return NoContent();
    }
}