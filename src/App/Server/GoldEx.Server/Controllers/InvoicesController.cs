using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Invoices.Base)]
public class InvoicesController(IInvoiceService service) : ApiControllerBase
{
    [HttpPost(ApiRoutes.Invoices.Create)]
    public async Task<IActionResult> CreateAsync(InvoiceRequestDto request, CancellationToken cancellationToken = default)
    {
        await service.CreateAsync(request, cancellationToken);
        return Created();
    }
}