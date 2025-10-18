using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Products;

namespace GoldEx.Client.Pages.Calculate.ViewModels;

public class CalculatorVm
{
    [Display(Name = "وزن")]
    [Required(ErrorMessage = "لطفا وزن را وارد کنید")]
    public decimal Weight { get; set; } = 1;

    public ProductType ProductType { get; set; } = ProductType.Gold;

    [Display(Name = "اجرت ساخت")]
    public decimal? Wage { get; set; }

    [Display(Name = "نوع اجرت")]
    public WageType? WageType { get; set; } = Shared.Enums.WageType.Percent;

    [Display(Name = "سود فروشنده")]
    [Required(ErrorMessage = "لطفا سود را وارد کنید")]
    public decimal ProfitPercent { get; set; } = 7;

    [Display(Name = "نرخ گرم")]
    [Required(ErrorMessage = "لطفا نرخ گرم را وارد کنید")]
    public decimal GramPrice { get; set; }

    [Display(Name = "نرخ تبدیل اجرت")]
    public decimal? WageExchangeRate { get; set; }

    [Display(Name = "نرخ تبدیل سنگ")]
    public decimal? StoneExchangeRate { get; set; }

    [Display(Name = "قیمت سنگ")]
    public decimal? StonePrice { get; set; }

    [Display(Name = "عیار")]
    public decimal Fineness { get; set; } = 750m;

    [Display(Name = "کسری عیار")]
    public decimal UsedGoldFinenessDeductionRate { get; set; } = 15;

    [Display(Name = "مالیات")]
    public decimal TaxPercent { get; set; } = 9;

    [Display(Name = "هزینه های جانبی")]
    public decimal? ExtraCosts { get; set; }

    [Display(Name = "واحد ارزی")]
    public GetPriceUnitTitleResponse? PriceUnit { get; set; }

    [Display(Name = "واحد ارزی اجرت")]
    public GetPriceUnitTitleResponse? WagePriceUnit { get; set; }

    [Display(Name = "واحد ارزی سنگ")]
    public GetPriceUnitTitleResponse? StonePriceUnit { get; set; }

    [Display(Name = "واحد سنجش طلا")]
    public GoldUnitType GoldUnitType { get; set; } = GoldUnitType.Gram;

    public decimal WeightAs750 => (Fineness / 750m) * Weight;

    public CalculatorVm CreateFrom(GetProductResponse response,
        GetPriceUnitTitleResponse? wagePriceUnit, GetPriceUnitTitleResponse? stonePriceUnit)
    {
        Weight = response.Weight;
        ProductType = response.ProductType;
        Wage = response.Wage;
        WageType = response.WageType;
        Fineness = response.Fineness;
        WagePriceUnit = wagePriceUnit;
        GoldUnitType = response.GoldUnitType;
        StonePriceUnit = stonePriceUnit;
        StonePrice = response.GemStones?.Sum(x => x.Cost) ?? 0;

        return this;
    }

    public void SetJewelry(decimal? jewelryProfitPercent, GetPriceUnitTitleResponse? defaultPriceUnit)
    {
        ProfitPercent = jewelryProfitPercent ?? 20;
        StonePriceUnit ??= defaultPriceUnit;
        Fineness = 750m;
        UsedGoldFinenessDeductionRate = 0;
        WageType = Shared.Enums.WageType.Fixed;
        WagePriceUnit ??= defaultPriceUnit;
    }

    public void SetGold(decimal? goldProfitPercent)
    {
        ProfitPercent = goldProfitPercent ?? 7;
        Fineness = 750m;
        StonePriceUnit = null;
        UsedGoldFinenessDeductionRate = 0;
        WageType = Shared.Enums.WageType.Percent;
        //WagePriceUnit = null;

        if (Wage is > 100)
            Wage = 0;
    }

    public void SetMoltenGold(decimal? commissionPercent)
    {
        Fineness = 750m;
        UsedGoldFinenessDeductionRate = 0;
        ProfitPercent = commissionPercent ?? 1.5m;
        WageType = null;
        WagePriceUnit = null;
        Wage = null;
        StonePriceUnit = null;
    }

    public void SetUsedGold(decimal? deductionRate)
    {
        UsedGoldFinenessDeductionRate = deductionRate ?? 15;
        Fineness = 750m;
        Wage = null;
        WageType = null;
        WagePriceUnit = null;
        StonePriceUnit = null;
        ProfitPercent = 0;
    }
}