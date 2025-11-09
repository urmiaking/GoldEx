using GoldEx.Sdk.Common.Definitions;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace GoldEx.Shared.Enums;

public enum PriceCatalog
{
    // Gold market
    [Display(Name = "آبشده نقدی")]
    [Market(MarketType.Gold)]
    Mazanne = 1,

    [Display(Name = "طلای 24 عیار (گرم)")]
    [Market(MarketType.Gold)]
    Geram24,

    [Display(Name = "طلای 18 عیار (گرم)")]
    [Market(MarketType.Gold)]
    Geram18,

    // Coin market
    [Display(Name = "سکه یک گرمی")]
    [Market(MarketType.Coin)]
    SekehYekGerami,

    [Display(Name = "حباب سکه گرمی")]
    [Market(MarketType.BubbleCoin)]
    SekehYekGeramiBubble,

    [Display(Name = "ربع سکه")]
    [Market(MarketType.Coin)]
    RobSeke,

    [Display(Name = "حباب ربع سکه")]
    [Market(MarketType.BubbleCoin)]
    RobSekeBubble,

    [Display(Name = "نیم سکه")]
    [Market(MarketType.Coin)]
    NimSeke,

    [Display(Name = "حباب نیم سکه")]
    [Market(MarketType.BubbleCoin)]
    NimSekeBubble,

    [Display(Name = "سکه امامی")]
    [Market(MarketType.Coin)]
    SekehEmami,

    [Display(Name = "حباب سکه امامی")]
    [Market(MarketType.BubbleCoin)]
    SekehEmamiBubble,

    [Display(Name = "سکه بهار آزادی")]
    [Market(MarketType.Coin)]
    SekehBaharAzadi,

    [Display(Name = "حباب سکه بهار آزادی")]
    [Market(MarketType.BubbleCoin)]
    SekehBaharAzadiBubble,

    // Parsian Coin market
    [Display(Name = "سکه 1,5 گرمی پارسیان")]
    [Market(MarketType.ParsianCoin)]
    Sekeh15GeramiParsian,

    [Display(Name = "سکه 1,4 گرمی پارسیان")]
    [Market(MarketType.ParsianCoin)]
    Sekeh14GeramiParsian,

    [Display(Name = "سکه 1,3 گرمی پارسیان")]
    [Market(MarketType.ParsianCoin)]
    Sekeh13GeramiParsian,

    [Display(Name = "سکه 1,2 گرمی پارسیان")]
    [Market(MarketType.ParsianCoin)]
    Sekeh12GeramiParsian,

    [Display(Name = "سکه 1 گرمی پارسیان")]
    [Market(MarketType.ParsianCoin)]
    Sekeh1GeramiParsian,

    [Display(Name = "سکه 900 سوتی")]
    [Market(MarketType.ParsianCoin)]
    Sekeh900Sooti,

    [Display(Name = "سکه 800 سوتی")]
    [Market(MarketType.ParsianCoin)]
    Sekeh800Sooti,

    [Display(Name = "سکه 700 سوتی")]
    [Market(MarketType.ParsianCoin)]
    Sekeh700Sooti,

    [Display(Name = "سکه 600 سوتی")]
    [Market(MarketType.ParsianCoin)]
    Sekeh600Sooti,

    [Display(Name = "سکه 500 سوتی")]
    [Market(MarketType.ParsianCoin)]
    Sekeh500Sooti,

    [Display(Name = "سکه 400 سوتی")]
    [Market(MarketType.ParsianCoin)]
    Sekeh400Sooti,

    [Display(Name = "سکه 300 سوتی")]
    [Market(MarketType.ParsianCoin)]
    Sekeh300Sooti,

    [Display(Name = "سکه 200 سوتی")]
    [Market(MarketType.ParsianCoin)]
    Sekeh200Sooti,

    [Display(Name = "سکه 100 سوتی")]
    [Market(MarketType.ParsianCoin)]
    Sekeh100Sooti,

    [Display(Name = "سکه 1,1 گرمی پارسیان")]
    [Market(MarketType.ParsianCoin)]
    Sekeh11GeramiParsian,

    // Currency market
    [Display(Name = "لاری گرجستان")]
    [Market(MarketType.Currency)]
    PriceGel,

    [Display(Name = "درام ارمنستان")]
    [Market(MarketType.Currency)]
    PriceAmd,

    [Display(Name = "منات آذربایجان")]
    [Market(MarketType.Currency)]
    PriceAzn,

    [Display(Name = "بات تایلند")]
    [Market(MarketType.Currency)]
    PriceThb,

    [Display(Name = "افغانی")]
    [Market(MarketType.Currency)]
    PriceAfn,

    [Display(Name = "دینار بحرین")]
    [Market(MarketType.Currency)]
    PriceBhd,

    [Display(Name = "کرون سوئد")]
    [Market(MarketType.Currency)]
    PriceSek,

    [Display(Name = "لیر سوریه")]
    [Market(MarketType.Currency)]
    PriceSyp,

    [Display(Name = "دینار عراق")]
    [Market(MarketType.Currency)]
    BankIqd,

    [Display(Name = "فرانک سوئیس")]
    [Market(MarketType.Currency)]
    SwitzerlandFranc,

    [Display(Name = "لیر ترکیه")]
    [Market(MarketType.Currency)]
    TurkeyLira,

    [Display(Name = "یوآن چین")]
    [Market(MarketType.Currency)]
    ChinaYuan,

    [Display(Name = "دلار کانادا")]
    [Market(MarketType.Currency)]
    CanadaDollar,

    [Display(Name = "دلار استرالیا")]
    [Market(MarketType.Currency)]
    AustralianDollar,

    [Display(Name = "دینار کویت")]
    [Market(MarketType.Currency)]
    KuwaitiDinar,

    [Display(Name = "یکصد ین ژاپن")]
    [Market(MarketType.Currency)]
    Yen100,

    [Display(Name = "پوند")]
    [Market(MarketType.Currency)]
    GbPound,

    [Display(Name = "درهم امارات")]
    [Market(MarketType.Currency)]
    UaeDeram,

    [Display(Name = "دلار")]
    [Market(MarketType.Currency)]
    UsDollar,

    [Display(Name = "روبل روسیه")]
    [Market(MarketType.Currency)]
    PriceRub,

    [Display(Name = "رینگیت مالزی")]
    [Market(MarketType.Currency)]
    PriceMyr,

    [Display(Name = "ریال عمان")]
    [Market(MarketType.Currency)]
    PriceOmr,

    [Display(Name = "ریال قطر")]
    [Market(MarketType.Currency)]
    PriceQar,

    [Display(Name = "روپیه پاکستان")]
    [Market(MarketType.Currency)]
    PricePkr,

    [Display(Name = "روپیه هند")]
    [Market(MarketType.Currency)]
    PriceInr,

    [Display(Name = "ریال عربستان")]
    [Market(MarketType.Currency)]
    SaudiArabiaRiyal,

    [Display(Name = "یورو")]
    [Market(MarketType.Currency)]
    Euro,

    [Display(Name = "انس جهانی طلا")]
    [Market(MarketType.Ounce)]
    Gold,

    [Display(Name = "انس نقره")]
    [Market(MarketType.Ounce)]
    Silver,
    
    [Display(Name = "انس پالادیوم")]
    [Market(MarketType.Ounce)]
    Palladium,
    
    [Display(Name = "انس پلاتین")]
    [Market(MarketType.Ounce)]
    Platinum,
}

// Custom attribute
[AttributeUsage(AttributeTargets.Field)]
public class MarketAttribute(MarketType marketType) : Attribute
{
    public MarketType MarketType { get; } = marketType;
}

// Extension for Market extraction
public static class PriceCatalogExtensions
{
    public static MarketType GetMarketType(this PriceCatalog value)
    {
        var member = value.GetType().GetMember(value.ToString())[0];
        var attribute = member.GetCustomAttribute<MarketAttribute>();
        return attribute?.MarketType ?? throw new InvalidOperationException($"Market type not defined for {value}");
    }
}