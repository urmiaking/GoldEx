using GoldEx.Shared.DTOs.CoinInstances;

namespace GoldEx.Shared.DTOs.InventoryEntries;

public record CreateCoinItemEntryRequest(
    int Quantity,
    decimal UnitPrice,
    CoinInstanceRequestDto CoinInstance);