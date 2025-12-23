using GoldEx.Client.Pages.Customers.ViewModels;
using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.CoinInstances;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class CoinPackageSpecVm
{
    [Display(Name = "وزن با وکیوم")]
    public decimal? VacuumedWeight { get; set; }

    [Display(Name = "کارگاه/نمایندگی")]
    public CustomerVm? Issuer { get; set; }

    [Display(Name = "کد استاندارد")]
    [Required(ErrorMessage = "کد استاندارد الزامی است")]
    public string? StandardCode { get; set; }

    [Display(Name = "رنگ کارت")]
    public string? CardColor { get; set; }

    public CoinPackageDto ToRequest()
    {
        return new CoinPackageDto(VacuumedWeight ?? throw new FluentValidation.ValidationException("وزن با وکیوم الزامی است"),
            StandardCode ?? throw new FluentValidation.ValidationException("کد استاندارد الزامی است"),
            CardColor,
            Issuer?.Id);
    }

    public static CoinPackageSpecVm CreateFrom(CoinPackageResponse response)
    {
        return new CoinPackageSpecVm
        {
            VacuumedWeight = response.VacuumedWeight,
            StandardCode = response.StandardCode,
            CardColor = response.CardColor,
            Issuer = response.Issuer != null ? CustomerVm.CreateFrom(response.Issuer) : null
        };
    }
}