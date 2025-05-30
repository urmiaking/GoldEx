using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Sdk.Server.Infrastructure.DTOs;

namespace GoldEx.Server.Infrastructure.Services.Price.DTOs.Signal;

public static class SignalApiResponseMapper
{
    public static List<PriceResponse> MapPrices(SignalApiResponse? response)
    {
        List<PriceResponse> priceResponses = [];

        if (response == null)
            return priceResponses;

        if (response.Data.Gold is not null)
        {
            priceResponses.AddRange(from item in response.Data.Gold.Data
                let currentValue = ParseDecimal(item.Close)
                let change = FormatChange(item.Change,
                    item.PercentChange)
                let lastUpdate = $"{item.JDate.FormatDateString()} {item.Time}"
                let iconUrl = item.IconUrl
                select new PriceResponse(item.PersianName?.ToPersianChars() ?? item.Name,
                    currentValue,
                    item.Unit,
                    lastUpdate,
                    change,
                    iconUrl,
                    GetMarketType(nameof(response.Data.Gold))));
        }

        if (response.Data.Currency is not null)
        {
            priceResponses.AddRange(from item in response.Data.Currency.Data
                let currentValue = ParseDecimal(item.Close)
                let change = FormatChange(item.Change,
                    item.PercentChange)
                let lastUpdate = $"{item.JDate.FormatDateString()} {item.Time}"
                let iconUrl = item.IconUrl
                //where item.Name is "usDollar" or "euro"
                select new PriceResponse(item.PersianName?.ToPersianChars() ?? item.Name,
                    currentValue,
                    item.Unit,
                    lastUpdate,
                    change,
                    iconUrl,
                    GetMarketType(nameof(response.Data.Currency))));
        }


        if (response.Data.Coin is not null)
        {
            priceResponses.AddRange(from item in response.Data.Coin.Data
                let currentValue = ParseDecimal(item.Close)
                let change = FormatChange(item.Change,
                    item.PercentChange)
                let lastUpdate = $"{item.JDate.FormatDateString()} {item.Time}"
                let iconUrl = item.IconUrl
                select new PriceResponse(item.PersianName?.ToPersianChars() ?? item.Name,
                    currentValue,
                    item.Unit,
                    lastUpdate,
                    change,
                    iconUrl,
                    GetMarketType(nameof(response.Data.Coin))));
        }

        if (response.Data.ParsianCoin is not null)
        {
            priceResponses.AddRange(from item in response.Data.ParsianCoin.Data
                let currentValue = ParseDecimal(item.Close)
                let change = FormatChange(item.Change,
                    item.PercentChange)
                let lastUpdate = $"{item.JDate.FormatDateString()} {item.Time}"
                let iconUrl = item.IconUrl
                select new PriceResponse(item.PersianName?.ToPersianChars() ?? item.Name,
                    currentValue,
                    item.Unit,
                    lastUpdate,
                    change,
                    iconUrl,
                    GetMarketType(nameof(response.Data.ParsianCoin))));
        }

        if (response.Data.BubbleCoin is not null)
        {
            priceResponses.AddRange(from item in response.Data.BubbleCoin?.Data
                let currentValue = ParseDecimal(item.Close)
                let change = FormatChange(item.Change,
                    item.PercentChange)
                let lastUpdate = $"{item.JDate.FormatDateString()} {item.Time}"
                let iconUrl = item.IconUrl
                select new PriceResponse(item.PersianName?.ToPersianChars() ?? item.Name,
                    currentValue,
                    item.Unit,
                    lastUpdate,
                    change,
                    iconUrl,
                    GetMarketType(nameof(response.Data.BubbleCoin))));
        }

        return priceResponses;
    }

    public static PriceResponse? GetGram18Price(SignalApiResponse? response)
    {
        if (response is null)
            return null;

        if (response.Data.Gold is null)
            throw new InvalidDataException("Gold part is missing from response. Request parameters is not correctly provided");

        var item = response.Data.Gold.Data.FirstOrDefault(x => x.Name == "geram18");

        if (item is null)
            throw new NotSupportedException("geram18 is not available in response");

        var title = item.PersianName?.ToPersianChars() ?? item.Name;
        var currentValue = ParseDecimal(item.Close);
        var unit = item.Unit;
        var change = FormatChange(item.Change, item.PercentChange);
        var lastUpdate = $"{item.JDate.FormatDateString()} {item.Time}";
        var iconUrl = item.IconUrl;

        return new PriceResponse(title, currentValue, unit, lastUpdate, change, iconUrl, GetMarketType(response.Data.Gold.Market));
    }

    public static PriceResponse? GetDollarPrice(SignalApiResponse? response)
    {
        if (response is null)
            return null;

        if (response.Data.Currency is null)
            throw new InvalidDataException("Currency part is missing from response. Request parameters is not correctly provided");

        var item = response.Data.Currency.Data.FirstOrDefault(x => x.Name == "usDollar");

        if (item is null)
            throw new NotSupportedException("usDollar is not available in response");

        var title = item.PersianName?.ToPersianChars() ?? item.Name;
        var currentValue = ParseDecimal(item.Close);
        var unit = item.Unit;
        var change = FormatChange(item.Change, item.PercentChange);
        var lastUpdate = $"{item.JDate.FormatDateString()} {item.Time}";
        var iconUrl = item.IconUrl;

        return new PriceResponse(title, currentValue, unit, lastUpdate, change, iconUrl, GetMarketType(response.Data.Currency.Market));
    }

    private static decimal ParseDecimal(long value) => value;

    private static string FormatChange(long change, double percentChange)
    {
        var changeString = change.ToString("N0");
        var percentChangeString = percentChange.ToString("0.00") + "%";

        return $"{changeString} ({percentChangeString})";
    }

    private static MarketType GetMarketType(string market)
    {
        return market switch
        {
            nameof(SignalApiResponse.Data.Coin) => MarketType.Coin,
            nameof(SignalApiResponse.Data.Currency) => MarketType.Currency,
            nameof(SignalApiResponse.Data.Gold) => MarketType.Gold,
            nameof(SignalApiResponse.Data.BubbleCoin) => MarketType.BubbleCoin,
            nameof(SignalApiResponse.Data.ParsianCoin) => MarketType.ParsianCoin,

            _ => throw new ArgumentOutOfRangeException()
        };
    }
}