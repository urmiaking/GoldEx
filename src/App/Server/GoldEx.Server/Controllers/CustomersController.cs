using GoldEx.Server.Common.Authorization;
using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Customers.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}, Demo")]
public class CustomersController(ICustomerService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Customers.Overview)]
    [DemoRead]
    public async Task<IActionResult> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var overview = await service.GetOverviewAsync(cancellationToken);
        return Ok(overview);
    }

    [HttpGet(ApiRoutes.Customers.GetList)]
    [DemoRead]
    public async Task<IActionResult> GetListAsync([FromQuery] RequestFilter filter, [FromQuery] CustomerFilter customerFilter, 
        CancellationToken cancellationToken)
    {
        var list = await service.GetListAsync(filter, customerFilter, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.Customers.Get)]
    [DemoRead]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpGet(ApiRoutes.Customers.GetByNationalId)]
    [DemoRead]
    public async Task<IActionResult> GetAsync(string nationalId, CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(nationalId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet(ApiRoutes.Customers.GetByPhoneNumber)]
    [DemoRead]
    public async Task<IActionResult> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        var item = await service.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet(ApiRoutes.Customers.GetByName)]
    [DemoRead]
    public async Task<IActionResult> GetByNameAsync(string? customerName, [FromQuery] CustomerType? type, CancellationToken cancellationToken)
    {
        var items = await service.GetByNameAsync(customerName, type, cancellationToken);
        return Ok(items);
    }

    [HttpGet(ApiRoutes.Customers.GetNames)]
    [DemoRead]
    public async Task<IActionResult> GetNamesAsync([FromQuery] string? name, [FromRoute] CustomerType type, CancellationToken cancellationToken)
    {
        var items = await service.GetNamesAsync(name, type, cancellationToken);
        return Ok(items);
    }

    [HttpGet(ApiRoutes.Customers.GenerateNationalId)]
    [DemoRead]
    public async Task<IActionResult> GenerateNationalIdAsync(CancellationToken cancellationToken)
    {
        var items = await service.GenerateNationalIdAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost(ApiRoutes.Customers.Create)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> CreateAsync(CustomerRequestDto request, CancellationToken cancellationToken)
    {
        var id = await service.CreateAsync(request, cancellationToken);
        return Ok(id);
    }

    [HttpPut(ApiRoutes.Customers.Update)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> UpdateAsync(Guid id, CustomerRequestDto request, CancellationToken cancellationToken)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return Ok();
    }

    [HttpDelete(ApiRoutes.Customers.Delete)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Ok();
    }
}
