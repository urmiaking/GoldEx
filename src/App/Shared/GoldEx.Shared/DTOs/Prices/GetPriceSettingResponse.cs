using GoldEx.Sdk.Common.Definitions;

namespace GoldEx.Shared.DTOs.Prices;

public record GetPriceSettingResponse(
    MarketType MarketType,
    List<PriceSettingDto>? Prices);