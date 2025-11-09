using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Prices;

public record ValidatePriceProviderRequest(
    Guid PriceId,
    PriceProviderType ProviderType,
    string ProviderSymbol
);