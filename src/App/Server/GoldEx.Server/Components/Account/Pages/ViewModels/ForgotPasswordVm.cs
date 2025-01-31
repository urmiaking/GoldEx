using System.ComponentModel.DataAnnotations;

namespace GoldEx.Server.Components.Account.Pages.ViewModels;

internal sealed class ForgotPasswordVm
{
    [Display(Name = "نام کاربری")]
    [Required(ErrorMessage = "لطفاً نام کاربری را وارد نمائید.")]
    public string Username { get; set; } = "";

    [Display(Name = "شماره تلفن همراه")]
    [Required(ErrorMessage = "لطفاً شماره تلفن همراه خود را وارد نمائید.")]
    public string PhoneNumber { get; set; } = "";
}
