using GoldEx.Sdk.Common.Definitions;

namespace GoldEx.Shared.DTOs.Prices;

public record GetPriceResponse(string Title, string Value, string Unit, string Change, string LastUpdate, string? IconFileBase64, MarketType Type);