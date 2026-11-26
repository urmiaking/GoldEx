using GoldEx.Server.Common.Authorization;
using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.FinancialAccounts.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}, Demo")]
public class FinancialAccountsController(IFinancialAccountService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.FinancialAccounts.GetAll)]
    [DemoRead]
    public async Task<IActionResult> GetAllAsync(Guid? customerId, CancellationToken cancellationToken = default)
    {
        var list = await service.GetAllAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.FinancialAccounts.GetList)]
    [DemoRead]
    public async Task<IActionResult> GetListAsync([FromQuery] RequestFilter filter,
        [FromQuery] FinancialAccountFilter financialAccountFilter, CancellationToken cancellationToken = default)
    {
        var pagedList = await service.GetListAsync(filter, financialAccountFilter, cancellationToken);
        return Ok(pagedList);
    }

    [HttpGet(ApiRoutes.FinancialAccounts.GetTitles)]
    [DemoRead]
    public async Task<IActionResult> GetTitlesAsync([FromQuery] Guid? customerId, [FromQuery] Guid? priceUnitId, CancellationToken cancellationToken = default)
    {
        var titles = await service.GetTitlesAsync(customerId, priceUnitId, cancellationToken);
        return Ok(titles);
    }

    [HttpGet(ApiRoutes.FinancialAccounts.GetCustomerAccounts)]
    [DemoRead]
    public async Task<IActionResult> GetCustomerAccountsAsync([FromRoute] Guid customerId, [FromQuery] Guid? priceUnitId, CancellationToken cancellationToken = default)
    {
        var titles = await service.GetCustomerAccountsAsync(customerId, priceUnitId, cancellationToken);
        return Ok(titles);
    }

    [HttpGet(ApiRoutes.FinancialAccounts.Get)]
    [DemoRead]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost(ApiRoutes.FinancialAccounts.Create)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> CreateAsync(FinancialAccountRequestDto request, CancellationToken cancellationToken)
    {
        await service.CreateAsync(request, cancellationToken);
        return Created();
    }

    [HttpPut(ApiRoutes.FinancialAccounts.Update)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> UpdateAsync(Guid id, FinancialAccountRequestDto request, CancellationToken cancellationToken)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return Ok();
    }

    [HttpDelete(ApiRoutes.FinancialAccounts.Delete)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Ok();
    }
}