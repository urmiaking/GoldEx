using System.Globalization;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Services.Price.DTOs.Tjgu;

public static class TjguResponseMapper
{
    public static List<PriceResponse> MapPrices(TjguResponse? data)
    {
        var responses = new List<PriceResponse>();

        if (data == null)
            return responses;

        var keyToCatalog = GetKeyToCatalogMapping();

        foreach (var kvp in keyToCatalog)
        {
            if (kvp.Value == PriceCatalog.UsDollar)
            {
                
            }

            if (data.Current.TryGetValue(kvp.Key, out var item) && decimal.TryParse(item.P.Replace(",", ""), out var currentValue))
            {
                var title = kvp.Value.GetDisplayName();
                var marketType = kvp.Value.GetMarketType();
                var lastUpdate = item.Ts;
                //var changeDirection = item.Dt switch
                //{
                //    "high" => "+",
                //    "low" => "-",
                //    _ => string.Empty
                //};
                var change = $"{item.D.Replace(",", "")} ({item.Dp}%)";
                var unit = UnitType.IRR.GetDisplayName();

                responses.Add(new PriceResponse(
                    Title: title,
                    CurrentValue: currentValue,
                    Unit: unit,
                    LastUpdate: DateTime.ParseExact(
                        lastUpdate,
                        "yyyy-MM-dd HH:mm:ss",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal
                    ),
                    Change: change,
                    IconUrl: null,
                    MarketType: marketType
                ));
            }
        }

        return responses;
    }

    private static Dictionary<string, PriceCatalog> GetKeyToCatalogMapping()
    {
        return new()
        {
            // Gold market
            { "mazanne", PriceCatalog.Mazanne },
            { "geram24", PriceCatalog.Geram24 }, // Assumed key for 24K gold per gram
            { "tgju_gold_irg18", PriceCatalog.Geram18 }, // Matches sample JSON for 18K gold

            // Coin market
            { "sekeyekgerami", PriceCatalog.SekehYekGerami },
            { "rob", PriceCatalog.RobSeke },
            { "nim", PriceCatalog.NimSeke },
            { "sekee", PriceCatalog.SekehEmami },
            { "sekeb", PriceCatalog.SekehBaharAzadi },

            // Parsian Coin market (assumed keys based on enum names; adjust if API uses different)
            { "seke15geramiparsian", PriceCatalog.Sekeh15GeramiParsian },
            { "seke14geramiparsian", PriceCatalog.Sekeh14GeramiParsian },
            { "seke13geramiparsian", PriceCatalog.Sekeh13GeramiParsian },
            { "seke12geramiparsian", PriceCatalog.Sekeh12GeramiParsian },
            { "seke1geramiparsian", PriceCatalog.Sekeh1GeramiParsian },
            { "seke900sooti", PriceCatalog.Sekeh900Sooti },
            { "seke800sooti", PriceCatalog.Sekeh800Sooti },
            { "seke700sooti", PriceCatalog.Sekeh700Sooti },
            { "seke600sooti", PriceCatalog.Sekeh600Sooti },
            { "seke500sooti", PriceCatalog.Sekeh500Sooti },
            { "seke400sooti", PriceCatalog.Sekeh400Sooti },
            { "seke300sooti", PriceCatalog.Sekeh300Sooti },
            { "seke200sooti", PriceCatalog.Sekeh200Sooti },
            { "seke100sooti", PriceCatalog.Sekeh100Sooti },
            { "seke11geramiparsian", PriceCatalog.Sekeh11GeramiParsian },

            // Currency market (keys prefixed with "price_" for IRR per foreign unit)
            { "price_gel", PriceCatalog.PriceGel },
            { "price_amd", PriceCatalog.PriceAmd },
            { "price_azn", PriceCatalog.PriceAzn },
            { "price_thb", PriceCatalog.PriceThb },
            { "price_afn", PriceCatalog.PriceAfn },
            { "price_bhd", PriceCatalog.PriceBhd },
            { "price_sek", PriceCatalog.PriceSek },
            { "price_syp", PriceCatalog.PriceSyp },
            { "bank_iqd", PriceCatalog.BankIqd },
            { "price_chf", PriceCatalog.SwitzerlandFranc },
            { "price_try", PriceCatalog.TurkeyLira },
            { "price_cny", PriceCatalog.ChinaYuan },
            { "price_cad", PriceCatalog.CanadaDollar },
            { "price_aud", PriceCatalog.AustralianDollar },
            { "price_kwd", PriceCatalog.KuwaitiDinar },
            { "price_jpy", PriceCatalog.Yen100 }, // Assumed for 100 JPY; adjust if separate
            { "price_gbp", PriceCatalog.GbPound },
            { "price_aed", PriceCatalog.UaeDeram },
            { "price_pab", PriceCatalog.UsDollar },
            { "price_rub", PriceCatalog.PriceRub },
            { "price_myr", PriceCatalog.PriceMyr },
            { "price_omr", PriceCatalog.PriceOmr },
            { "price_qar", PriceCatalog.PriceQar },
            { "price_pkr", PriceCatalog.PricePkr },
            { "price_inr", PriceCatalog.PriceInr },
            { "price_sar", PriceCatalog.SaudiArabiaRiyal },
            { "price_eur", PriceCatalog.Euro },

            { "xau", PriceCatalog.Gold }
        };
    }
}