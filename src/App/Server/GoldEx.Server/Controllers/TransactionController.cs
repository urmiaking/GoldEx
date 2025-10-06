using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Transactions.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class TransactionController(ITransactionService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Transactions.GetRemainingList)]
    public async Task<IActionResult> GetCustomerRemainingListAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var items = await service.GetCustomerRemainingListAsync(customerId, cancellationToken);
        return Ok(items);
    }
}