using GoldEx.Sdk.Common.Definitions;

namespace GoldEx.Shared.DTOs.Prices;

public record PriceResponse(
    string Title,
    decimal CurrentValue,
    string Unit,
    DateTime LastUpdate,
    string Change,
    string? IconUrl,
    MarketType MarketType);