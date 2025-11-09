using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Services.Price.DTOs.BrsApi;

public static class BrsApiResponseMapper
{
    public static List<PriceResponse> MapPrices(BrsApiResponse? response)
    {
        List<PriceResponse> result = [];
        if (response == null)
            return result;

        // 🟡 Gold
        if (response.Gold is not null)
        {
            result.AddRange(response.Gold.Select(item =>
                new PriceResponse(
                    Title: item.Name.ToPersianChars(),
                    CurrentValue: item.Price * 10,
                    Unit: UnitType.IRR.GetDisplayName(),
                    LastUpdate: $"{item.Date} {item.Time}".ToGregorianDateTime(),
                    Change: FormatChange(item.ChangeValue, item.ChangePercent),
                    IconUrl: null,
                    MarketType: GetMarketType(item.Symbol)
                )));
        }

        // 💵 Currency
        if (response.Currency is not null)
        {
            result.AddRange(response.Currency.Select(item =>
                new PriceResponse(
                    Title: item.Name.ToPersianChars(),
                    CurrentValue: item.Price,
                    Unit: UnitType.IRR.GetDisplayName(),
                    LastUpdate: $"{item.Date} {item.Time}".ToGregorianDateTime(),
                    Change: FormatChange(item.ChangeValue, item.ChangePercent),
                    IconUrl: null,
                    MarketType: MarketType.Currency
                )));
        }

        return result;
    }

    private static string FormatChange(decimal value, double percent)
    {
        var sign = value > 0 ? "+" : value < 0 ? "-" : "";
        return $"{sign}{Math.Abs(value):N0} ({percent:+0.##;-0.##;0}%)";
    }

    private static MarketType GetMarketType(string symbol)
    {
        if (symbol.Contains("COIN", StringComparison.OrdinalIgnoreCase))
            return MarketType.Coin;
        if (symbol.Contains("GOLD", StringComparison.OrdinalIgnoreCase))
            return MarketType.Gold;
        if (symbol.Contains("XAU", StringComparison.OrdinalIgnoreCase))
            return MarketType.Ounce;
        return MarketType.Gold;
    }
}