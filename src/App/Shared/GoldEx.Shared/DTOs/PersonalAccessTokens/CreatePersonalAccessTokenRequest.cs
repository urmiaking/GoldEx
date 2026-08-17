using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.DTOs.PersonalAccessTokens;

public class CreatePersonalAccessTokenRequest
{
    [Required(ErrorMessage = "نام کلید دسترسی الزامی است")]
    [StringLength(100, ErrorMessage = "نام کلید نباید بیش از ۱۰۰ کاراکتر باشد")]
    public string Name { get; set; } = string.Empty;

    public int? ExpireDays { get; set; }
}
