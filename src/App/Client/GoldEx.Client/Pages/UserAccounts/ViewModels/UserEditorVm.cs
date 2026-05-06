using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.UserAccounts;

namespace GoldEx.Client.Pages.UserAccounts.ViewModels;

public class UserEditorVm
{
    public Guid? Id { get; set; }

    [Display(Name = "نام و نام خانوادگی")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string? FullName { get; set; }

    [Display(Name = "نام کاربری")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string? Username { get; set; }

    [Display(Name = "رمز عبور")]
    public string? Password { get; set; }

    [Display(Name = "شماره همراه")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "ایمیل")]
    public string? Email { get; set; }

    [Display(Name = "نقش")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string? Role { get; set; }

    public static UserEditorVm CreateFrom(GetUserAccountResponse response)
    {
        return new UserEditorVm
        {
            Id = response.Id,
            Email = response.Email,
            FullName = response.FullName,
            Username = response.Username,
            PhoneNumber = response.PhoneNumber,
            Role = response.Role
        };
    }

    public UserAccountRequestDto ToRequest()
    {
        return new UserAccountRequestDto(Id, FullName ?? "", Username ?? "", Password, PhoneNumber, Email, Role ?? "");
    }
}