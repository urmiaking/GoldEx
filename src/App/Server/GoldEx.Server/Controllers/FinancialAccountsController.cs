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
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class FinancialAccountsController(IFinancialAccountService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.FinancialAccounts.GetAll)]
    public async Task<IActionResult> GetAllAsync(Guid? customerId, CancellationToken cancellationToken = default)
    {
        var list = await service.GetAllAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.FinancialAccounts.GetList)]
    public async Task<IActionResult> GetListAsync([FromQuery] RequestFilter filter,
        [FromQuery] FinancialAccountFilter financialAccountFilter, CancellationToken cancellationToken = default)
    {
        var pagedList = await service.GetListAsync(filter, financialAccountFilter, cancellationToken);
        return Ok(pagedList);
    }

    [HttpGet(ApiRoutes.FinancialAccounts.GetTitles)]
    public async Task<IActionResult> GetTitlesAsync([FromQuery] Guid? customerId, [FromQuery] Guid? priceUnitId, CancellationToken cancellationToken = default)
    {
        var titles = await service.GetTitlesAsync(customerId, priceUnitId, cancellationToken);
        return Ok(titles);
    }

    [HttpGet(ApiRoutes.FinancialAccounts.Get)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost(ApiRoutes.FinancialAccounts.Create)]
    public async Task<IActionResult> CreateAsync(FinancialAccountRequestDto request, CancellationToken cancellationToken)
    {
        await service.CreateAsync(request, cancellationToken);
        return Created();
    }

    [HttpPut(ApiRoutes.FinancialAccounts.Update)]
    public async Task<IActionResult> UpdateAsync(Guid id, FinancialAccountRequestDto request, CancellationToken cancellationToken)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return Ok();
    }

    [HttpDelete(ApiRoutes.FinancialAccounts.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Ok();
    }
}