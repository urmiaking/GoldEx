using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;

namespace GoldEx.Calculator.Client.ViewModels;

public sealed class TradeInVm
{
    // طلای کهنه دریافتی از مشتری
    public decimal UsedWeight { get; set; } = 1m;
    public decimal UsedFineness { get; set; } = 750m;
    public decimal UsedFinenessDeduction { get; set; } = 15m;
    public decimal UsedGramPrice { get; set; }

    public string? UsedItemName { get; set; } = "طلای کهنه تعویضی";

    // طلای نو خریداری شده
    public decimal NewWeight { get; set; } = 1m;
    public decimal NewFineness { get; set; } = 750m;
    public decimal NewGramPrice { get; set; }
    public WageType? NewWageType { get; set; } = WageType.Percent;
    public decimal? NewWage { get; set; } = 12m;
    public decimal NewProfitPercent { get; set; } = 7m;
    public decimal NewTaxPercent { get; set; } = 10m;
    public decimal? NewExtraCosts { get; set; }
    public string? NewItemName { get; set; } = "طلای نو (تعویض)";

    // محاسبات تحلیلی
    public decimal UsedEquivalent750Weight =>
        UsedFineness > 0
            ? (UsedWeight * Math.Max(0m, UsedFineness - UsedFinenessDeduction)) / 750m
            : 0m;

    public decimal UsedTotalValue
    {
        get
        {
            if (UsedWeight <= 0 || UsedGramPrice <= 0) return 0m;
            return CalculatorHelper.UsedProduct.Calculate(UsedWeight, UsedFineness, UsedFinenessDeduction, UsedGramPrice);
        }
    }

    public decimal NewRawPrice
    {
        get
        {
            if (NewWeight <= 0 || NewGramPrice <= 0) return 0m;
            return CalculatorHelper.Product.CalculateRawPrice(NewWeight, NewGramPrice, NewFineness, 1, ProductType.Gold);
        }
    }

    public decimal NewWageAmount
    {
        get
        {
            if (NewRawPrice <= 0 || !NewWage.HasValue) return 0m;
            return CalculatorHelper.Product.CalculateWage(NewRawPrice, NewWeight, NewWage, NewWageType, 1);
        }
    }

    public decimal NewProfitAmount
    {
        get
        {
            if (NewRawPrice <= 0) return 0m;
            return CalculatorHelper.Product.CalculateProfit(NewRawPrice, NewWageAmount, ProductType.Gold, NewProfitPercent);
        }
    }

    public decimal NewTaxAmount
    {
        get
        {
            if (NewRawPrice <= 0) return 0m;
            return CalculatorHelper.Product.CalculateTax(NewWageAmount, NewProfitAmount, NewTaxPercent, ProductType.Gold, 0);
        }
    }

    public decimal NewTotalValue
    {
        get
        {
            if (NewRawPrice <= 0) return 0m;
            return CalculatorHelper.Product.CalculateFinalPrice(NewRawPrice, NewWageAmount, NewProfitAmount, NewTaxAmount, NewExtraCosts, ProductType.Gold);
        }
    }

    public decimal NetDifference => NewTotalValue - UsedTotalValue;

    public decimal NetWeightDifference => NewWeight - UsedEquivalent750Weight;
}
