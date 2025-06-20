using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Invoices.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class InvoicesController(IInvoiceService service) : ApiControllerBase
{
    [HttpPost(ApiRoutes.Invoices.Create)]
    public async Task<IActionResult> CreateAsync(InvoiceRequestDto request, CancellationToken cancellationToken = default)
    {
        await service.CreateAsync(request, cancellationToken);
        return Created();
    }

    [HttpGet(ApiRoutes.Invoices.GetList)]
    public async Task<IActionResult> GetListAsync([FromQuery] RequestFilter filter, [FromQuery] Guid? customerId, CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(filter, customerId, cancellationToken);
        return Ok(list);
    }
}