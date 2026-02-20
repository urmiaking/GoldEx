using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Components.Calculator.ViewModels;

public class ReverseCalculatorVm
{
    [Display(Name = "نرخ واحد")]
    [Required(ErrorMessage = "لطفا نرخ واحد را وارد کنید")]
    public decimal UnitPrice { get; set; }

    [Display(Name = "واحد سنجش طلا")]
    public GoldUnitType GoldUnitType { get; set; } = GoldUnitType.Gram;

    [Display(Name = "عیار")]
    [Required(ErrorMessage = "لطفا عیار را وارد کنید")]
    public decimal? Fineness { get; set; } = 750m;

    [Display(Name = "قیمت")]
    public decimal? Price { get; set; }

    [Display(Name = "سود")]
    [Required(ErrorMessage = "لطفا سود را وارد کنید")]
    public decimal? ProfitPercent { get; set; } = 1.5m;

    [Display(Name = "واحد ارزی")]
    public GetPriceUnitTitleResponse? PriceUnit { get; set; }
}