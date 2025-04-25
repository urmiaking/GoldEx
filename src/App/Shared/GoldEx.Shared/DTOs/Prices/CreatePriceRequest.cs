using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Prices;

public record CreatePriceRequest(Guid Id,
    string Title,
    string Value,
    string Unit,
    string Change,
    string LastUpdate,
    string? IconFileBase64,
    MarketType Type,
    UnitType? UnitType);