using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Products.ViewModels;

public class GemStoneVm
{
    [Display(Name = "کد")]
    [Required(ErrorMessage = "وارد كردن {0} الزامی است")]
    public string Code { get; set; } = default!;

    [Display(Name = "نوع سنگ")]
    [Required(ErrorMessage = "وارد كردن {0} الزامی است")]
    public string Type { get; set; } = default!;

    [Display(Name = "رنگ")]
    [Required(ErrorMessage = "وارد كردن {0} الزامی است")]
    public string Color { get; set; } = default!;

    [Display(Name = "برش")]
    public string? Cut { get; set; }

    [Display(Name = "قيراط")]
    [Required(ErrorMessage = "وارد كردن {0} الزامی است")]
    public double Carat { get; set; } = default!;

    [Display(Name = "پاكي")]
    public string? Purity { get; set; } 
}