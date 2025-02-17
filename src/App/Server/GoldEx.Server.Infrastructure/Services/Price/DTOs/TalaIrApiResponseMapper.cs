using System.Globalization;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Sdk.Server.Infrastructure.DTOs;
using GoldEx.Sdk.Server.Infrastructure.DTOs.Enums;

namespace GoldEx.Server.Infrastructure.Services.Price.DTOs;

public static class TalaIrApiResponseMapper
{
    public static List<PriceResponse> MapCoinPrices(TalaIrApiResponse? response)
    {
        List<PriceResponse> priceResponses = [];

        if (response == null)
            return priceResponses;

        if (response.CoinPrice != null)
        {
            foreach (var coin in response.CoinPrice)
            {
                if (double.TryParse(coin.Value.Value.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var currentValue))
                {
                    priceResponses.Add(new PriceResponse(
                        coin.Value.Title.ToPersianChars(), // Title (Coin Name)
                        currentValue, // Current Value
                        coin.Value.LastUpdate, // Last Update
                        coin.Value.Change, // Daily Change Rate
                        PriceType.Coin // Price Type
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
                if (double.TryParse(gold.Value.Value.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var currentValue))
                {
                    priceResponses.Add(new PriceResponse(
                        gold.Value.Title.ToPersianChars(), // Title (Gold Name)
                        currentValue, // Current Value
                        gold.Value.LastUpdate, // Last Update
                        gold.Value.Change, // Daily Change Rate
                        PriceType.Gold // Price Type
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
                if (double.TryParse(currency.Value.Value.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var currentValue))
                {
                    priceResponses.Add(new PriceResponse(
                        currency.Value.Title.ToPersianChars(), // Title (Currency Name)
                        currentValue, // Current Value
                        currency.Value.LastUpdate, // Last Update
                        currency.Value.Change, // Daily Change Rate
                        PriceType.Currency // Price Type
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

        if (double.TryParse(gold18.Value.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var currentValue))
        {
            return new PriceResponse(
                gold18.Title.ToPersianChars(), // Title
                currentValue, // Current Value
                gold18.LastUpdate, // Last Update
                gold18.Change, // Daily Change Rate
                PriceType.Gold // Price Type
            );
        }

        return null;
    }

    public static PriceResponse? GetDollarPrice(TalaIrApiResponse? content)
    {
        if (content?.CurrencyPrice is null)
            return null;

        var dollar = content.CurrencyPrice.FirstOrDefault(c => c.Key == "arz_dolar").Value;

        if (double.TryParse(dollar.Value.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var currentValue))
        {
            return new PriceResponse(
                dollar.Title.ToPersianChars(), // Title
                currentValue, // Current Value
                dollar.LastUpdate, // Last Update
                dollar.Change, // Daily Change Rate
                PriceType.Currency // Price Type
            );
        }
        return null;
    }
}