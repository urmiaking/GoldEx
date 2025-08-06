using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.LedgerAccounts;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.LedgerAccounts.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class LedgerAccountsController(ILedgerAccountService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.LedgerAccounts.GetList)]
    public async Task<IActionResult> GetListAsync([FromQuery] Guid? customerId, CancellationToken cancellationToken)
    {
        var list = await service.GetListAsync(customerId, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.LedgerAccounts.Get)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost(ApiRoutes.LedgerAccounts.Create)]
    public async Task<IActionResult> CreateAsync(LedgerAccountRequestDto request, CancellationToken cancellationToken)
    {
        await service.CreateAsync(request, cancellationToken);
        return Created();
    }

    [HttpPut(ApiRoutes.LedgerAccounts.Update)]
    public async Task<IActionResult> UpdateAsync(Guid id, LedgerAccountRequestDto request, CancellationToken cancellationToken)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete(ApiRoutes.LedgerAccounts.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}