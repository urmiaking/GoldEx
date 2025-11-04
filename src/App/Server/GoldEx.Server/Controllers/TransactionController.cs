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
    public async Task<IActionResult> GetListAsync([FromQuery] TransactionFilter transactionFilter,
        [FromQuery] RequestFilter requestFilter, CancellationToken cancellationToken = default)
    {
        var items = await service.GetListAsync(transactionFilter, requestFilter, cancellationToken);
        return Ok(items);
    }

    [HttpGet(ApiRoutes.Transactions.GetRemainingList)]
    public async Task<IActionResult> GetCustomerRemainingListAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var items = await service.GetCustomerRemainingListAsync(customerId, cancellationToken);
        return Ok(items);
    }

    [HttpGet(ApiRoutes.Transactions.GetFinancialAccountBalance)]
    public async Task<IActionResult> GetFinancialAccountBalanceAsync(Guid financialAccountId, CancellationToken cancellationToken = default)
    {
        var item = await service.GetFinancialAccountBalanceAsync(financialAccountId, cancellationToken);
        return Ok(item);
    }
}