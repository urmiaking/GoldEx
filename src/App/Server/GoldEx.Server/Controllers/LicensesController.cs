using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Shared.DTOs.Licenses;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VHDLicenseManager.Requests;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Licenses.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class LicensesController(ILicenseService service, IServerLicenseService licenseServerService) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Licenses.Get)]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken = default)
    {
        var result = await service.GetLicenseAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost(ApiRoutes.Licenses.Register)]
    public async Task<IActionResult> RegisterAsync(RegisterProductRequest request, CancellationToken cancellationToken = default)
    {
        await service.RegisterProductAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost(ApiRoutes.Licenses.SendToken)]
    public async Task<IActionResult> SendTokenAsync([FromRoute] string phoneNumber, CancellationToken cancellationToken = default)
    {
        await service.SendTokenAsync(phoneNumber, cancellationToken);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost(ApiRoutes.Licenses.CallbackUrl)]
    public async Task<IActionResult> Callback(LicenseCallbackRequest request, CancellationToken cancellationToken = default)
    {
        await licenseServerService.ActivateProductAsync(request, cancellationToken);
        return Ok();
    }
}