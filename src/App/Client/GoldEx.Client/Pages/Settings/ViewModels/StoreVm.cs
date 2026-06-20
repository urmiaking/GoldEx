using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace GoldEx.Client.Pages.Settings.ViewModels;

public class StoreVm
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "نام فروشگاه الزامی است")]
    [StringLength(100, ErrorMessage = "نام فروشگاه حداکثر ۱۰۰ کاراکتر است")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "شناسه فروشگاه (Slug) الزامی است")]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "شناسه باید فقط شامل حروف کوچک انگلیسی، اعداد و خط تیره (-) باشد")]
    [StringLength(50, ErrorMessage = "شناسه فروشگاه حداکثر ۵۰ کاراکتر است")]
    public string Slug { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }
    public IBrowserFile? LogoFile { get; set; }

    public string? BackgroundImageUrl { get; set; }
    public IBrowserFile? BackgroundImageFile { get; set; }

    public bool IsActive { get; set; } = true;
    public int Index { get; set; }
}
