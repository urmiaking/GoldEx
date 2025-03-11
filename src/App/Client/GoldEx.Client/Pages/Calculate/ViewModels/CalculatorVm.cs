using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.Calculate.ViewModels;

public class CalculatorVm
{
    [Display(Name = "وزن")]
    [Required(ErrorMessage = "لطفا وزن را وارد کنید")]
    public double Weight { get; set; }

    public ProductType ProductType { get; set; } = ProductType.Gold;

    [Display(Name = "اجرت ساخت")]
    public double? Wage { get; set; }

    [Display(Name = "نوع اجرت")]
    public WageType? WageType { get; set; } = GoldEx.Shared.Enums.WageType.Percent;

    [Display(Name = "سود فروشنده")]
    [Required(ErrorMessage = "لطفا سود را وارد کنید")]
    public double Profit { get; set; } // based on percent

    [Display(Name = "نرخ گرم")]
    [Required(ErrorMessage = "لطفا نرخ گرم را وارد کنید")]
    public double GramPrice { get; set; }

    [Display(Name = "نرخ دلار")]
    public double? UsDollarPrice { get; set; }

    [Display(Name = "عیار")]
    public CaratType CaratType { get; set; } = CaratType.Eighteen;

    public double Tax { get; set; } // based on percent

    public double? AdditionalPrices { get; set; }
}