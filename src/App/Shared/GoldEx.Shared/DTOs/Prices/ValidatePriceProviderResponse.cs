using GoldEx.Sdk.Common.Definitions;

namespace GoldEx.Shared.DTOs.Prices;

public record ValidatePriceProviderResponse(
    bool IsSupported,
    bool MarketTypeMatched,
    string? Message,
    MarketType? ProviderMarketType,
    PriceResponse? Sample
);