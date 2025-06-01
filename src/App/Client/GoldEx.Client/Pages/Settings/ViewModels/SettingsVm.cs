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
    public decimal TaxPercent { get; set; }

    [Display(Name = "سود طلا")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public decimal GoldProfitPercent { get; set; }
    
    [Display(Name = "سود جواهر")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public decimal JewelryProfitPercent { get; set; }

    [Display(Name = "زمان بروز رسانی قیمت ها")]
    public TimeSpan PriceUpdateInterval { get; set; }

    public UpdateSettingRequest ToRequest()
    {
        return new UpdateSettingRequest(InstitutionName, Address, PhoneNumber, TaxPercent, GoldProfitPercent, JewelryProfitPercent, PriceUpdateInterval);
    }

    public static SettingsVm CreateFromRequest(GetSettingResponse response)
    {
        return new SettingsVm
        {
            Id = response.Id,
            InstitutionName = response.InstitutionName,
            Address = response.Address,
            PhoneNumber = response.PhoneNumber,
            TaxPercent = response.TaxPercent,
            GoldProfitPercent = response.GoldProfitPercent,
            JewelryProfitPercent = response.JewelryProfitPercent
        };
    }
}