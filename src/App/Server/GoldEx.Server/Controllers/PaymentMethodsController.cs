using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.PaymentMethods;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.PaymentMethods.Base)]
public class PaymentMethodsController(IPaymentMethodService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.PaymentMethods.GetAll)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await service.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet(ApiRoutes.PaymentMethods.GetList)]
    public async Task<IActionResult> GetListAsync(CancellationToken cancellationToken = default)
    {
        var items = await service.GetListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet(ApiRoutes.PaymentMethods.Get)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost(ApiRoutes.PaymentMethods.Create)]
    public async Task<IActionResult> CreateAsync(CreatePaymentMethodRequest request, CancellationToken cancellationToken = default)
    {
        await service.CreateAsync(request, cancellationToken);
        return Created();
    }

    [HttpPut(ApiRoutes.PaymentMethods.Update)]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdatePaymentMethodRequest request, CancellationToken cancellationToken = default)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPut(ApiRoutes.PaymentMethods.UpdateStatus)]
    public async Task<IActionResult> UpdateStatusAsync(Guid id, UpdatePaymentMethodStatusRequest request, CancellationToken cancellationToken = default)
    {
        await service.UpdateStatusAsync(id, request, cancellationToken);
        return NoContent();
    }
}