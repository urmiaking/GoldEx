using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.Settings;

namespace GoldEx.Client.Pages.Settings.ViewModels;

public class SettingsVm
{
    public Guid Id { get; set; }

    [Display(Name = "نام گالری")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string InstitutionName { get; set; } = default!;

    [Display(Name = "آدرس")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string Address { get; set; } = default!;

    [Display(Name = "شماره تلفن")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string PhoneNumber { get; set; } = default!;

    [Display(Name = "مالیات")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public double Tax { get; set; }

    [Display(Name = "سود طلا")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public double GoldProfit { get; set; }
    
    [Display(Name = "سود جواهر")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public double JewelryProfit { get; set; }

    public UpdateSettingsRequest ToRequest()
    {
        return new UpdateSettingsRequest(InstitutionName, Address, PhoneNumber, Tax, GoldProfit, JewelryProfit);
    }

    public static SettingsVm CreateFromRequest(GetSettingsResponse response)
    {
        return new SettingsVm
        {
            Id = response.Id,
            InstitutionName = response.InstitutionName,
            Address = response.Address,
            PhoneNumber = response.PhoneNumber,
            Tax = response.Tax,
            GoldProfit = response.GoldProfit,
            JewelryProfit = response.JewelryProfit
        };
    }
}