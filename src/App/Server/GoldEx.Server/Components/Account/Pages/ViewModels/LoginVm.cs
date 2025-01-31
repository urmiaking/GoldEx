using System.ComponentModel.DataAnnotations;

namespace GoldEx.Server.Components.Account.Pages.ViewModels;

public record LoginVm
{
    [Display(Name = "نام کاربری")]
    [Required(ErrorMessage = "لطفاً {0} را وارد نمائید.")]
    public string Username { get; set; } = "";

    [Display(Name = "کلمه عبور")]
    [Required(ErrorMessage = "لطفاً {0} را وارد نمائید.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Display(Name = "مرا بخاطر بسپار")]
    public bool RememberMe { get; set; }
}
