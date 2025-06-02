using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Prices;

public record GetPriceResponse(
    Guid Id,
    string Title,
    string Value,
    string Unit,
    string Change,
    string LastUpdate,
    bool HasIcon,
    MarketType Type,
    UnitType? UnitType);