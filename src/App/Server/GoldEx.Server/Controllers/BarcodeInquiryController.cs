using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.BarcodeInquiries.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class BarcodeInquiryController(IBarcodeInquiryService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.BarcodeInquiries.GetList)]
    public async Task<IActionResult> GetListAsync([FromQuery] string? barcode, CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(barcode, cancellationToken);
        return Ok(list);
    }

    [HttpPost(ApiRoutes.BarcodeInquiries.Inquiry)]
    public async Task<IActionResult> InquiryAsync(string barcode, CancellationToken cancellationToken = default)
    {
        await service.InquiryAsync(barcode, cancellationToken);
        return NoContent();
    }
}