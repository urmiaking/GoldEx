using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.InventoryStocks.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class InventoryStocksController(IInventoryStockService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.InventoryStocks.GetList)]
    public async Task<IActionResult> GetListAsync([FromQuery] RequestFilter filter, [FromQuery] InventoryFilter inventoryFilter,
        CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(filter, inventoryFilter, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.InventoryStocks.GetAvailableProducts)]
    public async Task<IActionResult> GetAvailableProductsAsync([FromQuery] CalculatorFilterRequest calculatorFilter, [FromQuery] RequestFilter requestFilter,
        CancellationToken cancellationToken = default)
    {
        var list = await service.GetAvailableProductsAsync(calculatorFilter, requestFilter, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.InventoryStocks.GetInventoryWeightChart)]
    public async Task<IActionResult> GetInventoryWeightChartAsync(WarehouseActionType actionType, CancellationToken cancellationToken = default)
    {
        var list = await service.GetInventoryWeightChartAsync(actionType, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.InventoryStocks.GetInvoiceInventoryItems)]
    public async Task<IActionResult> GetInvoiceInventoryItemsAsync(Guid invoiceId,
        [FromQuery] RequestFilter requestFilter, CancellationToken cancellationToken = default)
    {
        var list = await service.GetInvoiceInventoryItemsAsync(invoiceId, requestFilter, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.InventoryStocks.GetTraces)]
    public async Task<IActionResult> GetInventoryStockTracesAsync(Guid itemId, ItemType itemType,
        [FromQuery] RequestFilter requestFilter, CancellationToken cancellationToken = default)
    {
        var list = await service.GetInventoryStockTracesAsync(itemId, itemType, requestFilter, cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.InventoryStocks.GetAvailableItemAmount)]
    public async Task<IActionResult> GetAvailableItemAmountAsync(Guid itemId, ItemType itemType, CancellationToken cancellationToken = default)
    {
        var result = await service.GetAvailableItemAmountAsync(itemId, itemType, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.InventoryStocks.GetTitle)]
    public async Task<IActionResult> GetTitleAsync(ItemType itemType, Guid id, CancellationToken cancellationToken = default)
    {
        var result = await service.GetTitleAsync(itemType, id, cancellationToken);
        return Ok(result);
    }

    [HttpDelete(ApiRoutes.InventoryStocks.DeleteProduct)]
    public async Task<IActionResult> DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        await service.DeleteProductAsync(productId, cancellationToken);
        return NoContent();
    }
}