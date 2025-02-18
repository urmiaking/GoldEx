using GoldEx.Sdk.Common.Definitions;

namespace GoldEx.Shared.DTOs;

public record GetPriceResponse(string Title, string Value, string Change, string LastUpdate, PriceType Type);