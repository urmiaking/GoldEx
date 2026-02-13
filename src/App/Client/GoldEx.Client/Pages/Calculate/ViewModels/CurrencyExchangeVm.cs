using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Calculate.ViewModels;

public class CurrencyExchangeVm
{
    [Display(Name = "ارز مبدا")]
    [Required(ErrorMessage = "لطفا {0} را انتخاب کنید")]
    public GetPriceUnitTitleResponse? SourcePriceUnit { get; set; }

    [Display(Name = "ارز مقصد")]
    [Required(ErrorMessage = "لطفا {0} را انتخاب کنید")]
    public GetPriceUnitTitleResponse? DestinationPriceUnit { get; set; }

    [Display(Name = "مبلغ")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public decimal? SourceAmount { get; set; } = 1;

    public decimal? ExchangeRate { get; set; }

    public decimal? DestinationAmount { get; set; }

    [Display(Name = "کارمزد")]
    public decimal? Fee { get; set; } = 0.5m;

    [Display(Name = "نوع کارمزد")]
    public WageType? FeeType { get; set; } = WageType.Percent;

    public GetPriceUnitTitleResponse? FeePriceUnit { get; set; }

    public decimal? FeeExchangeRate { get; set; }

    public decimal? FeeInDestination { get; set; }

    public decimal? FinalDestinationAmount { get; set; }
}
