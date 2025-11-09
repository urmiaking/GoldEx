using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Prices;

public record UpdatePriceSettingRequest(byte[] IconContent,
    PriceProviderType ProviderType,
    string? ProviderSymbol,
    bool IsProviderEnabled);