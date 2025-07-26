using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.PaymentVouchers.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class PaymentVouchersController(IPaymentVoucherService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.PaymentVouchers.GetList)]
    public async Task<IActionResult> GetListAsync(
        [FromQuery] RequestFilter filter,
        [FromQuery] PaymentVoucherFilter voucherFilter,
        [FromQuery] Guid? customerId,
        CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(filter, voucherFilter, customerId, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.PaymentVouchers.Get)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var voucher = await service.GetAsync(id, cancellationToken);
        return Ok(voucher);
    }

    [HttpGet(ApiRoutes.PaymentVouchers.GetByNumber)]
    public async Task<IActionResult> GetAsync(long voucherNumber, CancellationToken cancellationToken = default)
    {
        var voucher = await service.GetAsync(voucherNumber, cancellationToken);
        return Ok(voucher);
    }

    [HttpPost(ApiRoutes.PaymentVouchers.Create)]
    public async Task<IActionResult> CreateAsync(PaymentVoucherRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await service.CreateAsync(request, cancellationToken);
        return Created();
    }

    [HttpPut(ApiRoutes.PaymentVouchers.Update)]
    public async Task<IActionResult> UpdateAsync(Guid id, PaymentVoucherRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return Ok();
    }

    [HttpDelete(ApiRoutes.PaymentVouchers.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Ok();
    }

    [HttpGet(ApiRoutes.PaymentVouchers.GetLastNumber)]
    public async Task<IActionResult> GetLastNumberAsync(CancellationToken cancellationToken = default)
    {
        var lastNumber = await service.GetLastNumberAsync(cancellationToken);
        return Ok(lastNumber);
    }
}