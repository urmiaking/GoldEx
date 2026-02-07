using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.LicensePayments;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.LicensePayments.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class LicensePaymentsController(ILicensePaymentService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.LicensePayments.GetList)]
    public async Task<IActionResult> GetListAsync(CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpPost(ApiRoutes.LicensePayments.Create)]
    public async Task<IActionResult> CreateAsync(LicensePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        await service.CreateAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPut(ApiRoutes.LicensePayments.SetStatus)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> SetStatusAsync(Guid id, LicensePaymentStatus status,
        CancellationToken cancellationToken = default)
    {
        await service.SetStatusAsync(id, status, cancellationToken);
        return Accepted();
    }
}