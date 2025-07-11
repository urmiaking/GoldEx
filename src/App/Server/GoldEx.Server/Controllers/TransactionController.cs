using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Transactions.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class TransactionController(ITransactionService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Transactions.GetList)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> GetListAsync([FromQuery] RequestFilter filter, [FromQuery] Guid? customerId, CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(filter, customerId, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.Transactions.Get)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpGet(ApiRoutes.Transactions.GetByNumber)]
    public async Task<IActionResult> GetAsync(int number, CancellationToken cancellationToken = default)
    {
        var transaction = await service.GetAsync(number, cancellationToken);
        return Ok(transaction);
    }

    [HttpPost(ApiRoutes.Transactions.Create)]
    public async Task<IActionResult> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        await service.CreateAsync(request, cancellationToken);
        return Created();
    }

    [HttpPut(ApiRoutes.Transactions.Update)]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return Ok();
    }

    [HttpDelete(ApiRoutes.Transactions.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id, bool deletePermanently = false, CancellationToken cancellationToken = default)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Ok();
    }

    [HttpGet(ApiRoutes.Transactions.GetLastNumber)]
    public async Task<IActionResult> GetLastNumberAsync(CancellationToken cancellationToken = default)
    {
        var transactionNumber = await service.GetLastNumberAsync(cancellationToken);
        return Ok(transactionNumber);
    }
}