using GoldEx.Sdk.Common.Definitions;

namespace GoldEx.Shared.DTOs.Prices;

public record PriceProviderSymbolDto(string Symbol,
    string Title,
    MarketType MarketType);

public record GetPriceProviderCatalogResponse(
    List<PriceProviderSymbolDto> Items
);