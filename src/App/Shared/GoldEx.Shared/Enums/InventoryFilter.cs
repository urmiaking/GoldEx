namespace GoldEx.Shared.Enums;

public record InventoryFilter(WarehouseActionType? ActionType, ItemType? ItemType, Guid? CategoryId, DateTime? Start, DateTime? End, Guid? InventoryEntryId = null);