using GoldEx.Sdk.Common.Definitions;

namespace GoldEx.Shared.DTOs;

public record GetPriceResponse(string Title, string Value, string Unit, string Change, string LastUpdate, string? IconUrl, MarketType Type);