using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.CustomerTransfers;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.CustomerTransfers.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class CustomerTransfersController(ICustomerTransferVoucherService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.CustomerTransfers.GetList)]
    public async Task<IActionResult> GetListAsync(
        [FromQuery] RequestFilter filter,
        [FromQuery] CustomerTransferVoucherFilter voucherFilter,
        CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(filter, voucherFilter, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.CustomerTransfers.Get)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var voucher = await service.GetAsync(id, cancellationToken);
        return Ok(voucher);
    }

    [HttpGet(ApiRoutes.CustomerTransfers.GetByNumber)]
    public async Task<IActionResult> GetAsync(long voucherNumber, CancellationToken cancellationToken = default)
    {
        var voucher = await service.GetAsync(voucherNumber, cancellationToken);
        return Ok(voucher);
    }

    [HttpPost(ApiRoutes.CustomerTransfers.Create)]
    public async Task<IActionResult> CreateAsync(CreateCustomerTransferVoucherRequest request,
        CancellationToken cancellationToken = default)
    {
        await service.CreateAsync(request, cancellationToken);
        return Created();
    }

    [HttpPut(ApiRoutes.CustomerTransfers.Update)]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateCustomerTransferVoucherRequest request,
        CancellationToken cancellationToken = default)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return Ok();
    }

    [HttpDelete(ApiRoutes.CustomerTransfers.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Ok();
    }

    [HttpGet(ApiRoutes.CustomerTransfers.GetLastNumber)]
    public async Task<IActionResult> GetLastNumberAsync(CancellationToken cancellationToken = default)
    {
        var lastNumber = await service.GetLastNumberAsync(cancellationToken);
        return Ok(lastNumber);
    }
}
