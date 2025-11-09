using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Prices;

public record PriceSettingDto(
    Guid Id,
    string Title,
    MarketType MarketType,
    bool IsActive,
    bool IsPinned,
    PriceProviderType? ProviderType,
    string? ProviderSymbol,
    bool? IsProviderEnabled);