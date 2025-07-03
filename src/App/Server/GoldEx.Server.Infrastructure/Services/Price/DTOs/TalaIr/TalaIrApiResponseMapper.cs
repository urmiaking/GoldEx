using System.Globalization;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Sdk.Server.Infrastructure.DTOs;

namespace GoldEx.Server.Infrastructure.Services.Price.DTOs.TalaIr;

public static class TalaIrApiResponseMapper
{
    public static List<PriceResponse> MapAllPrices(TalaIrApiResponse? response)
    {
        var list = new List<PriceResponse>();

        list.AddRange(MapGoldPrices(response));
        list.AddRange(MapCurrencyPrices(response));
        list.AddRange(MapCoinPrices(response));

        return list;
    }

    public static List<PriceResponse> MapCoinPrices(TalaIrApiResponse? response)
    {
        List<PriceResponse> priceResponses = [];

        if (response == null)
            return priceResponses;

        if (response.CoinPrice != null)
        {
            foreach (var coin in response.CoinPrice)
            {
                if (decimal.TryParse(coin.Value.Value.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var currentValue))
                {
                    priceResponses.Add(new PriceResponse(
                        coin.Value.Title.ToPersianChars(), // Title (Coin Name)
                        currentValue, // Current Value
                        "تومان",
                        coin.Value.LastUpdate, // Last Update
                        coin.Value.Change, // Daily Change Rate
                        "",
                        MarketType.Coin // Market Type
                    ));
                }
            }
        }

        return priceResponses;
    }

    public static List<PriceResponse> MapGoldPrices(TalaIrApiResponse? response)
    {
        List<PriceResponse> priceResponses = [];

        if (response == null)
            return priceResponses;

        if (response.GoldPrice != null)
        {
            foreach (var gold in response.GoldPrice)
            {
                if (decimal.TryParse(gold.Value.Value.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var currentValue))
                {
                    priceResponses.Add(new PriceResponse(
                        gold.Value.Title.ToPersianChars(), // Title (Gold Name)
                        currentValue, // Current Value
                        "تومان",
                        gold.Value.LastUpdate, // Last Update
                        gold.Value.Change, // Daily Change Rate
                        "",
                        MarketType.Gold // Market Type
                    ));
                }
            }

        }

        return priceResponses;
    }

    public static List<PriceResponse> MapCurrencyPrices(TalaIrApiResponse? response)
    {
        List<PriceResponse> priceResponses = [];

        if (response == null)
            return priceResponses;

        if (response.CurrencyPrice != null)
        {
            foreach (var currency in response.CurrencyPrice)
            {
                if (decimal.TryParse(currency.Value.Value.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var currentValue))
                {
                    priceResponses.Add(new PriceResponse(
                        currency.Value.Title.ToPersianChars(), // Title (Currency Name)
                        currentValue, // Current Value
                        "تومان",
                        currency.Value.LastUpdate, // Last Update
                        currency.Value.Change, // Daily Change Rate
                        "",
                        MarketType.Currency // Price Type
                    ));
                }
            }
        }

        return priceResponses;
    }

    public static PriceResponse? GetGram18Price(TalaIrApiResponse? content)
    {
        if (content?.GoldPrice is null)
            return null;

        var gold18 = content.GoldPrice.FirstOrDefault(g => g.Key == "gold_18k").Value;

        if (decimal.TryParse(gold18.Value.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var currentValue))
        {
            return new PriceResponse(
                gold18.Title.ToPersianChars(), // Title
                currentValue, // Current Value
                "تومان",
                gold18.LastUpdate, // Last Update
                gold18.Change, // Daily Change Rate
                "",
                MarketType.Gold // Market Type
            );
        }

        return null;
    }

    public static PriceResponse? GetDollarPrice(TalaIrApiResponse? content)
    {
        if (content?.CurrencyPrice is null)
            return null;

        var dollar = content.CurrencyPrice.FirstOrDefault(c => c.Key == "arz_dolar").Value;

        if (decimal.TryParse(dollar.Value.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var currentValue))
        {
            return new PriceResponse(
                dollar.Title.ToPersianChars(), // Title
                currentValue, // Current Value
                "تومان",
                dollar.LastUpdate, // Last Update
                dollar.Change, // Daily Change Rate
                "",
                MarketType.Currency // Price Type
            );
        }
        return null;
    }
}