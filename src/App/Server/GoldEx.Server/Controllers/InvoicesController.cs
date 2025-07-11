using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
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
        await service.SetAsync(request, cancellationToken);
        return Created();
    }

    [HttpDelete(ApiRoutes.Invoices.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id, [FromQuery] bool deleteProducts, CancellationToken cancellationToken = default)
    {
        await service.DeleteAsync(id, deleteProducts, cancellationToken);
        return Ok();
    }

    [HttpGet(ApiRoutes.Invoices.GetList)]
    public async Task<IActionResult> GetListAsync([FromQuery] RequestFilter filter, [FromQuery] InvoiceFilter invoiceFilter,
        [FromQuery] Guid? customerId, CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(filter, invoiceFilter, customerId, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.Invoices.Get)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpGet(ApiRoutes.Invoices.GetByNumber)]
    public async Task<IActionResult> GetAsync(long invoiceNumber, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(invoiceNumber, cancellationToken);
        return Ok(item);
    }

    [HttpGet(ApiRoutes.Invoices.GetLastNumber)]
    public async Task<IActionResult> GetLastNumberAsync(CancellationToken cancellationToken = default)
    {
        var lastNumber = await service.GetLastNumberAsync(cancellationToken);
        return Ok(lastNumber);
    }
}