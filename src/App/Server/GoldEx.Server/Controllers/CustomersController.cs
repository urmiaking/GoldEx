using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Customers.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class CustomersController(ICustomerService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Customers.GetList)]
    public async Task<IActionResult> GetListAsync([FromQuery] RequestFilter filter, [FromQuery] CustomerFilter customerFilter, 
        CancellationToken cancellationToken)
    {
        var list = await service.GetListAsync(filter, customerFilter, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.Customers.Get)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpGet(ApiRoutes.Customers.GetByNationalId)]
    public async Task<IActionResult> GetAsync(string nationalId, CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(nationalId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet(ApiRoutes.Customers.GetByPhoneNumber)]
    public async Task<IActionResult> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        var item = await service.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
        return Ok(item);
    }

    [HttpPost(ApiRoutes.Customers.Create)]
    public async Task<IActionResult> CreateAsync(CustomerRequestDto request, CancellationToken cancellationToken)
    {
        var id = await service.CreateAsync(request, cancellationToken);
        return Ok(id);
    }

    [HttpPut(ApiRoutes.Customers.Update)]
    public async Task<IActionResult> UpdateAsync(Guid id, CustomerRequestDto request, CancellationToken cancellationToken)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return Ok();
    }

    [HttpDelete(ApiRoutes.Customers.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Ok();
    }
}