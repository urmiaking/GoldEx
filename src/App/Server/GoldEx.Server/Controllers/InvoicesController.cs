using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Api;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Invoices.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class InvoicesController(IInvoiceService service, IServerInvoiceService serverService) : ApiControllerBase
{
    [HttpPost(ApiRoutes.Invoices.Create)]
    public async Task<IActionResult> CreateAsync(InvoiceRequestDto request, CancellationToken cancellationToken = default)
    {
        await service.CreateAsync(request, cancellationToken);
        return Created();
    }

    [HttpPut(ApiRoutes.Invoices.Update)]
    public async Task<IActionResult> UpdateAsync(Guid id, InvoiceRequestDto request, CancellationToken cancellationToken = default)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete(ApiRoutes.Invoices.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet(ApiRoutes.Invoices.GetList)]
    public async Task<IActionResult> GetListAsync([FromQuery] RequestFilter filter, [FromQuery] InvoiceFilter invoiceFilter,
        [FromQuery] Guid? customerId, CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(filter, invoiceFilter, customerId, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.Invoices.GetCustomerInvoices)]
    public async Task<IActionResult> GetCustomerInvoicesAsync(Guid customerId,
        Guid priceUnitId,
        [FromQuery] RequestFilter filter,
        CancellationToken cancellationToken = default)
    {
        var list = await service.GetCustomerInvoicesAsync(customerId, priceUnitId, filter, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.Invoices.Get)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpGet(ApiRoutes.Invoices.GetByNumber)]
    public async Task<IActionResult> GetAsync(long invoiceNumber, InvoiceType invoiceType, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(invoiceNumber, invoiceType, cancellationToken);
        return Ok(item);
    }

    [HttpGet(ApiRoutes.Invoices.GetLastNumber)]
    public async Task<IActionResult> GetLastNumberAsync(InvoiceType invoiceType, CancellationToken cancellationToken = default)
    {
        var lastNumber = await service.GetLastNumberAsync(invoiceType, cancellationToken);
        return Ok(lastNumber);
    }

    [HttpPost(ApiRoutes.Invoices.SendReminder)]
    public async Task<IActionResult> SendReminderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.SendReminderAsync(id, cancellationToken);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet(ApiRoutes.Invoices.DownloadPdf)]
    public async Task<IActionResult> DownloadPdfAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pdfBytes = await serverService.GeneratePdfAsync(id, cancellationToken);
        return File(pdfBytes, "application/pdf", $"invoice-{id}.pdf");
    }
}