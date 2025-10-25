using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.Products;

namespace GoldEx.Client.Pages.Products.ViewModels;

public class GemStoneVm
{
    [Display(Name = "کد")]
    public string? Code { get; set; } = default!;

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
    public decimal Carat { get; set; } = default!;

    [Display(Name = "پاكي")]
    public string? Purity { get; set; }

    [Display(Name = "نرخ")]
    [Required(ErrorMessage = "وارد كردن {0} الزامی است")]
    public decimal Cost { get; set; }

    public GemStoneRequestDto ToRequest()
    {
        return new GemStoneRequestDto(Code, Type, Color, Cut, Carat, Cost, Purity);
    }

    public static GemStoneVm CreateFrom(GetGemStoneResponse dto)
    {
        return new GemStoneVm
        {
            Code = dto.Code,
            Type = dto.Type,
            Color = dto.Color,
            Cut = dto.Cut,
            Carat = dto.Carat,
            Purity = dto.Purity,
            Cost = dto.Cost
        };
    }
}