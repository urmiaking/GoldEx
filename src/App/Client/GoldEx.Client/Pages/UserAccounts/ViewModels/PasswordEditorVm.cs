using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.UserAccounts.ViewModels;

public class PasswordEditorVm
{
    [Display(Name = "رمز عبور فعلی")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string? Password { get; set; }

    [Display(Name = "رمز عبور جدید")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string? NewPassword { get; set; }

    [Display(Name = "تکرار رمز عبور جدید")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    [Compare(nameof(NewPassword), ErrorMessage = "رمز عبور جدید با تکرار آن مطابقت ندارد")]
    public string? ConfirmPassword { get; set; }
}