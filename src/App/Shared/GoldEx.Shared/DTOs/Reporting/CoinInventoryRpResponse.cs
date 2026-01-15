using GoldEx.Shared.DTOs.CoinInstances;

namespace GoldEx.Shared.DTOs.Reporting;

public record CoinInventoryRpResponse(
    DateTime DateTime,
    GetCoinInstanceResponse CoinInstance,
    decimal CurrentAmount,
    decimal SoldAmount);