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
}