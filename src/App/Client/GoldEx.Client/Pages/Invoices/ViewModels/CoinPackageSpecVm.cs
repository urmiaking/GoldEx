using GoldEx.Client.Pages.Customers.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class CoinPackageSpecVm
{
    [Display(Name = "وزن با وکیوم")]
    public decimal? VacuumedWeight { get; set; }

    [Display(Name = "کارگاه/نمایندگی")]
    public CustomerVm? Issuer { get; set; }

    [Display(Name = "کد استاندارد")]
    public string? SerialNumber { get; set; }

    [Display(Name = "رنگ کارت")]
    public string? CardColor { get; set; }
}