using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Transactions.Base)]
public class TransactionController(ITransactionClientService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Transactions.GetList)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> GetListAsync([FromQuery] RequestFilter filter, CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(filter, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.Transactions.Get)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transaction = await service.GetAsync(id, cancellationToken);
        return transaction is null ? NotFound() : Ok(transaction);
    }

    [HttpGet(ApiRoutes.Transactions.GetByNumber)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> GetAsync(int number, CancellationToken cancellationToken = default)
    {
        var transaction = await service.GetAsync(number, cancellationToken);
        return transaction is null ? NotFound() : Ok(transaction);
    }

    [HttpPost(ApiRoutes.Transactions.Create)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        await service.CreateAsync(request, cancellationToken);
        return Ok();
    }

    [HttpPut(ApiRoutes.Transactions.Update)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        await service.UpdateAsync(id, request, cancellationToken);
        return Ok();
    }

    [HttpDelete(ApiRoutes.Transactions.Delete)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> DeleteAsync(Guid id, bool deletePermanently = false, CancellationToken cancellationToken = default)
    {
        await service.DeleteAsync(id, deletePermanently, cancellationToken);
        return Ok();
    }

    [HttpGet(ApiRoutes.Transactions.GetPendingItems)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> GetPendingItemsAsync(DateTime checkPointDate,
        CancellationToken cancellationToken = default)
    {
        var list = await service.GetPendingsAsync(checkPointDate, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.Transactions.GetLatestTransactionNumber)]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> GetLatestTransactionNumberAsync(CancellationToken cancellationToken = default)
    {
        var transactionNumber = await service.GetLatestTransactionNumberAsync(cancellationToken);
        return Ok(transactionNumber);
    }
}