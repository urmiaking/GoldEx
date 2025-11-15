namespace GoldEx.Shared.DTOs.InventoryEntries;

public record CreateInventoryEntryRequest(List<CreateInventoryStockRequest> Stocks);