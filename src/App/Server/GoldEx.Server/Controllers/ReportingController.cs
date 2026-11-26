using GoldEx.Server.Common.Authorization;
using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Reporting.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}, Demo")]
public class ReportingController(IReportingService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Reporting.GetProfitAnalysis)]
    [DemoRead]
    public async Task<IActionResult> GetProfitAnalysisAsync([FromQuery] ProfitAnalysisRpRequest request,
        CancellationToken cancellationToken = default)
        => Ok(await service.GetProfitAnalysisAsync(request, cancellationToken));

    [HttpGet(ApiRoutes.Reporting.GetLedgerAccountStatements)]
    [DemoRead]
    public async Task<IActionResult> GetLedgerAccountStatementsAsync(
        [FromQuery] LedgerAccountStatementRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetLedgerAccountStatementsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetLedgerAccountTrialBalance)]
    [DemoRead]
    public async Task<IActionResult> GetLedgerAccountTrialBalanceAsync(
        [FromQuery] LedgerAccountTrialBalanceRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetLedgerAccountTrialBalanceAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetCustomerRemainingBalance)]
    [DemoRead]
    public async Task<IActionResult> GetCustomerRemainingBalanceAsync(
        [FromQuery] CustomerRemainingBalanceRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetCustomerRemainingBalanceAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetCustomerTransactions)]
    [DemoRead]
    public async Task<IActionResult> GetCustomerTransactionsAsync(
        [FromQuery] CustomerTransactionRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetCustomerTransactionsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetCustomerComprehensive)]
    [DemoRead]
    public async Task<IActionResult> GetCustomerComprehensiveAsync(
        [FromQuery] CustomerComprehensiveRpRequest request, CancellationToken cancellationToken = default)
        => Ok(await service.GetCustomerComprehensiveAsync(request, cancellationToken));

    [HttpGet(ApiRoutes.Reporting.GetSellInvoices)]
    [DemoRead]
    public async Task<IActionResult> GetSellInvoicesAsync(
        [FromQuery] SellInvoiceRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetSellInvoicesAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetPurchaseInvoices)]
    [DemoRead]
    public async Task<IActionResult> GetPurchaseInvoicesAsync(
        [FromQuery] PurchaseInvoiceRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetPurchaseInvoicesAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetPayments)]
    [DemoRead]
    public async Task<IActionResult> GetPaymentsAsync(
        [FromQuery] PaymentRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetPaymentsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetInvoicePayments)]
    [DemoRead]
    public async Task<IActionResult> GetInvoicePaymentsAsync(
        [FromQuery] InvoicePaymentRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetInvoicePaymentsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetInventoryKardex)]
    [DemoRead]
    public async Task<IActionResult> GetInventoryKardexAsync(
        [FromQuery] InventoryKardexRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetInventoryKardexAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetProductInventory)]
    [DemoRead]
    public async Task<IActionResult> GetProductInventoryAsync(
        [FromQuery] ProductInventoryRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetProductInventoryAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetCoinInventory)]
    [DemoRead]
    public async Task<IActionResult> GetCoinInventoryAsync(
        [FromQuery] CoinInventoryRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetCoinInventoryAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetCurrencyInventory)]
    [DemoRead]
    public async Task<IActionResult> GetCurrencyInventoryAsync(
        [FromQuery] CurrencyInventoryRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetCurrencyInventoryAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetUsedGoldHiddenProfit)]
    [DemoRead]
    public async Task<IActionResult> GetUsedGoldHiddenProfitAsync(
        [FromQuery] UsedGoldHiddenProfitRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetUsedGoldHiddenProfitAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetCategorySalesSummary)]
    [DemoRead]
    public async Task<IActionResult> GetCategorySalesSummaryAsync(
        [FromQuery] CategorySalesRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetCategorySalesSummaryAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetSoldProductItems)]
    [DemoRead]
    public async Task<IActionResult> GetSoldProductItemsAsync(
        [FromQuery] SoldProductItemRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetSoldProductItemsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Reporting.GetCategorySalesComparison)]
    [DemoRead]
    public async Task<IActionResult> GetCategorySalesComparisonAsync(
        [FromQuery] CategorySalesComparisonRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetCategorySalesComparisonAsync(request, cancellationToken);
        return Ok(result);
    }
}
