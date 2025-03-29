using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Customers.Base)]
public class CustomersController(ICustomerClientService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Customers.GetList)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> GetListAsync([FromQuery] RequestFilter filter, CancellationToken cancellationToken)
    {
        var list = await service.GetListAsync(filter, cancellationToken);
        return Ok(list);
    }

    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    [HttpGet(ApiRoutes.Customers.Get)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    [HttpGet(ApiRoutes.Customers.GetByNationalId)]
    public async Task<IActionResult> GetAsync(string nationalId, CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(nationalId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    [HttpGet(ApiRoutes.Customers.GetByPhoneNumber)]
    public async Task<IActionResult> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        var item = await service.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    [HttpPost(ApiRoutes.Customers.Create)]
    public async Task<IActionResult> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        await service.CreateAsync(request, cancellationToken);
        return Ok();
    }

    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    [HttpPut(ApiRoutes.Customers.Update)]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return Ok();
    }

    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    [HttpDelete(ApiRoutes.Customers.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.DeleteAsync(id, false, cancellationToken);
        return Ok();
    }

    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    [HttpGet(ApiRoutes.Customers.GetPendingItems)]
    public async Task<IActionResult> GetPendingsAsync(DateTime checkPointDate, CancellationToken cancellationToken)
    {
        var list = await service.GetPendingsAsync(checkPointDate, cancellationToken);

        return Ok(list);
    }
}