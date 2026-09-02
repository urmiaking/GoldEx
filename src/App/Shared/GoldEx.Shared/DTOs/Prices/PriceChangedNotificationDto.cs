using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Prices;

public record PriceChangedNotificationDto(
    Guid Id,
    string Title,
    decimal OldValue,
    decimal NewValue,
    decimal CurrentValue,
    string Value,
    string Unit,
    string Change,
    double ChangePercent,
    PriceChangeDirection Direction,
    DateTime? LastUpdate,
    MarketType MarketType,
    UnitType? UnitType,
    PriceCatalog? PriceCatalog = null
);
