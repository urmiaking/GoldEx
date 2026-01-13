using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Reporting;

public record InventoryKardexRpResponse(
    DateTime DateTime,
    WarehouseActionType ActionType,
    string Description,
    decimal Amount,
    GoldUnitType? GoldUnitType,
    string? PriceUnit,
    string SourceUrl);