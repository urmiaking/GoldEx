namespace GoldEx.Calculator.Client.ViewModels;

public enum CoinType
{
    Emami,        // تمام بهار آزادی (امامی)
    BaharAzadi,   // تمام بهار آزادی (طرح قدیم)
    Nim,          // نیم بهار آزادی
    Rob,          // ربع بهار آزادی
    Gerami        // سکه یک گرمی
}

public sealed class CoinItemModel
{
    public CoinType Type { get; init; }
    public required string Title { get; init; }
    public decimal WeightGrams { get; init; }
    public decimal Fineness { get; init; } = 900m;
    public decimal MarketPrice { get; set; }
    public decimal MintFee { get; init; } = 7000m; // حق ضرب مصوب بانک مرکزی (تومان)

    public decimal CalculateIntrinsicValue(decimal ounceUsd, decimal usdRate)
    {
        if (ounceUsd <= 0 || usdRate <= 0 || WeightGrams <= 0) return 0m;
        // وزن به گرم * عیار ۹۰۰ * قیمت هر گرم طلای ۲۴ عیار جهانی به تومان
        const decimal troyOunceGrams = 31.1034768m;
        var gold24PricePerGramToman = (ounceUsd * usdRate) / troyOunceGrams;
        var pureGoldWeight = WeightGrams * (Fineness / 1000m);
        return (gold24PricePerGramToman * pureGoldWeight) + MintFee;
    }

    public decimal GetBubbleAmount(decimal ounceUsd, decimal usdRate)
    {
        var intrinsic = CalculateIntrinsicValue(ounceUsd, usdRate);
        if (intrinsic <= 0 || MarketPrice <= 0) return 0m;
        return MarketPrice - intrinsic;
    }

    public decimal GetBubblePercent(decimal ounceUsd, decimal usdRate)
    {
        var intrinsic = CalculateIntrinsicValue(ounceUsd, usdRate);
        if (intrinsic <= 0 || MarketPrice <= 0) return 0m;
        return ((MarketPrice - intrinsic) / intrinsic) * 100m;
    }
}

public sealed class CoinBubbleVm
{
    public decimal OuncePriceUsd { get; set; }
    public decimal UsdRateToman { get; set; }

    public List<CoinItemModel> Coins { get; set; } =
    [
        new() { Type = CoinType.Emami, Title = "تمام سکه امامی (طرح جدید)", WeightGrams = 8.133m, Fineness = 900m },
        new() { Type = CoinType.BaharAzadi, Title = "تمام سکه بهار آزادی (طرح قدیم)", WeightGrams = 8.133m, Fineness = 900m },
        new() { Type = CoinType.Nim, Title = "نیم سکه بهار آزادی", WeightGrams = 4.066m, Fineness = 900m },
        new() { Type = CoinType.Rob, Title = "ربع سکه بهار آزادی", WeightGrams = 2.033m, Fineness = 900m },
        new() { Type = CoinType.Gerami, Title = "سکه یک گرمی بانک مرکزی", WeightGrams = 1.000m, Fineness = 900m }
    ];
}
