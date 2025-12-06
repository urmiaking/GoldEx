namespace GoldEx.Shared.DTOs.InventoryEntries;

public record CreateCoinItemEntryRequest(Guid CoinId, int Quantity, decimal UnitPrice);