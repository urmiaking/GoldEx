namespace GoldEx.Shared.DTOs.InventoryEntries;

public record CreateCoinItemRequest(Guid CoinId, int Quantity, decimal UnitPrice);