using GoldEx.Shared.DTOs.Licenses;
using GoldEx.Shared.DTOs.Settings;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Register.ViewModels;

public class RegisterFormVm
{
    [Display(Name = "نام گالری")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string InstitutionName { get; set; } = default!;

    [Display(Name = "آدرس")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string Address { get; set; } = default!;

    [Display(Name = "شماره تلفن مجموعه")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string InstitutionPhoneNumber { get; set; } = default!;

    [Display(Name = "شماره همراه")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string PhoneNumber { get; set; } = default!;

    [Display(Name = "کد فعالسازی")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string? Token { get; set; }

    [Display(Name = "لوگوی گالری")]
    public IBrowserFile? IconFile { get; set; }

    [Display(Name = "لوگوی گالری")]
    public byte[]? IconContent { get; set; }

    public bool HasIcon { get; set; }

    public static RegisterFormVm CreateFrom(GetSettingResponse response)
    {
        return new RegisterFormVm
        {
            InstitutionName = response.InstitutionName,
            Address = response.Address,
            InstitutionPhoneNumber = response.PhoneNumber,
            HasIcon = response.HasIcon
        };
    }

    public RegisterProductRequest ToRequest()
    {
        if (Token is null)
            throw new ArgumentNullException();

        return new RegisterProductRequest(InstitutionName, Address, InstitutionPhoneNumber, PhoneNumber, Token, IconContent);
    }
}