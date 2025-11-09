using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Prices;

public record UpdatePriceProviderRequest(PriceProviderType ProviderType,
    string ProviderSymbol,
    bool IsEnabled);