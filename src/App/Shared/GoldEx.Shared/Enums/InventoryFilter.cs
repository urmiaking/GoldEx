namespace GoldEx.Shared.Enums;

public record InventoryFilter(WarehouseActionType ActionType, ItemType ItemType, DateTime? Start, DateTime? End);