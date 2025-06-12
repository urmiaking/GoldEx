using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.PriceUnits;

namespace GoldEx.Client.Pages.Calculate.ViewModels;

public class CalculatorVm
{
    [Display(Name = "وزن")]
    [Required(ErrorMessage = "لطفا وزن را وارد کنید")]
    public decimal Weight { get; set; }

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
    public decimal? ExchangeRate { get; set; }

    [Display(Name = "عیار")]
    public CaratType CaratType { get; set; } = CaratType.Eighteen;

    [Display(Name = "مالیات")]
    public decimal TaxPercent { get; set; } = 9;

    [Display(Name = "هزینه های جانبی")]
    public decimal? ExtraCosts { get; set; }

    [Display(Name = "واحد ارزی")]
    public GetPriceUnitTitleResponse? PriceUnit { get; set; }

    [Display(Name = "واحد ارزی اجرت")]
    public GetPriceUnitTitleResponse? WagePriceUnit { get; set; }
}