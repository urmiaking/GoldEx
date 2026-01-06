using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Reporting.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class ReportingController(IReportingService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Reporting.GetLedgerAccountStatements)]
    public async Task<IActionResult> GetLedgerAccountStatementsAsync(
        [FromQuery] LedgerAccountStatementRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetLedgerAccountStatementsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetLedgerAccountTrialBalance)]
    public async Task<IActionResult> GetLedgerAccountTrialBalanceAsync(
        [FromQuery] LedgerAccountTrialBalanceRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetLedgerAccountTrialBalanceAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetCustomerRemainingBalance)]
    public async Task<IActionResult> GetCustomerRemainingBalanceAsync(
        [FromQuery] CustomerRemainingBalanceRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetCustomerRemainingBalanceAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetCustomerTransactions)]
    public async Task<IActionResult> GetCustomerTransactionsAsync(
        [FromQuery] CustomerTransactionRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetCustomerTransactionsAsync(request, cancellationToken);
        return Ok(result);
    }
}