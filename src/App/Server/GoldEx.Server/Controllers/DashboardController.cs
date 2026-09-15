using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Dashboard.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class DashboardController(IDashboardService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Dashboard.GetTodaySales)]
    public async Task<IActionResult> GetTodaySalesAsync(CancellationToken cancellationToken = default)
    {
        var result = await service.GetTodaySalesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Dashboard.GetCustomerBalances)]
    public async Task<IActionResult> GetCustomerBalancesSummaryAsync(CancellationToken cancellationToken = default)
    {
        var result = await service.GetCustomerBalancesSummaryAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Dashboard.GetTradeTrend30Days)]
    public async Task<IActionResult> GetTradeTrend30DaysAsync(CancellationToken cancellationToken = default)
    {
        var result = await service.GetTradeTrend30DaysAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Dashboard.GetTopUnpaidInvoices)]
    public async Task<IActionResult> GetTopUnpaidInvoicesAsync([FromQuery] int count = 5, CancellationToken cancellationToken = default)
    {
        var result = await service.GetTopUnpaidInvoicesAsync(count, cancellationToken);
        return Ok(result);
    }
}
