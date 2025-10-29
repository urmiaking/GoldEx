using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.InventoryStocks;

public record GetInventoryStockItemResponse(
    WarehouseActionType ActionType,
    string Description,
    decimal Amount,
    string? PriceUnit,
    GoldUnitType? GoldUnitType,
    DateTime DateTime
    );