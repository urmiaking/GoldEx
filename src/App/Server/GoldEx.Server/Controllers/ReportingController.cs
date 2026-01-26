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

    [HttpGet(ApiRoutes.Reporting.GetSellInvoices)]
    public async Task<IActionResult> GetSellInvoicesAsync(
        [FromQuery] SellInvoiceRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetSellInvoicesAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetPurchaseInvoices)]
    public async Task<IActionResult> GetPurchaseInvoicesAsync(
        [FromQuery] PurchaseInvoiceRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetPurchaseInvoicesAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetPayments)]
    public async Task<IActionResult> GetPaymentsAsync(
        [FromQuery] PaymentRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetPaymentsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetInvoicePayments)]
    public async Task<IActionResult> GetInvoicePaymentsAsync(
        [FromQuery] InvoicePaymentRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetInvoicePaymentsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetInventoryKardex)]
    public async Task<IActionResult> GetInventoryKardexAsync(
        [FromQuery] InventoryKardexRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetInventoryKardexAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetProductInventory)]
    public async Task<IActionResult> GetProductInventoryAsync(
        [FromQuery] ProductInventoryRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetProductInventoryAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetCoinInventory)]
    public async Task<IActionResult> GetCoinInventoryAsync(
        [FromQuery] CoinInventoryRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetCoinInventoryAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetCurrencyInventory)]
    public async Task<IActionResult> GetCurrencyInventoryAsync(
        [FromQuery] CurrencyInventoryRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetCurrencyInventoryAsync(request, cancellationToken);
        return Ok(result);
    }
}