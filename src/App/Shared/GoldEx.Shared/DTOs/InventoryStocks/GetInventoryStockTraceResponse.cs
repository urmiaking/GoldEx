using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.InventoryStocks;

public record GetInventoryStockTraceResponse(
    DateTime DateTime,
    WarehouseActionType ActionType,
    string Description,
    decimal Amount,
    GoldUnitType? GoldUnitType,
    string? PriceUnit,
    string SourceUrl);