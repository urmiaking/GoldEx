using GoldEx.Shared.DTOs.StoneTypes;
using GoldEx.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Settings.ViewModels;

public class StoneTypeVm
{
    public Guid Id { get; set; }
    public int Index { get; set; }

    [Display(Name = "عنوان")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string Title { get; set; } = default!;

    [Display(Name = "عنوان انگلیسی")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string EnTitle { get; set; } = default!;

    [Display(Name = "نماد (سمبل)")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    [MaxLength(4, ErrorMessage = "{0} نمی تواند بیشتر از {1} کاراکتر باشد")]
    public string Symbol { get; set; } = default!;

    [Display(Name = "نوع سنگ")]
    [Required(ErrorMessage = "لطفا {0} را تعیین کنید")]
    public StoneKind Kind { get; set; } = StoneKind.Precious;

    public bool IsActive { get; set; } = true;

    public static StoneTypeVm CreateFrom(GetStoneTypeResponse response)
    {
        return new StoneTypeVm
        {
            Id = response.Id,
            Title = response.Title,
            EnTitle = response.EnTitle,
            Symbol = response.Symbol,
            Kind = response.Kind,
            IsActive = response.IsActive
        };
    }

    public static CreateStoneTypeRequest ToCreateRequest(StoneTypeVm item)
    {
        return new CreateStoneTypeRequest(item.Title, item.EnTitle, item.Symbol, item.Kind);
    }

    public static UpdateStoneTypeRequest ToUpdateRequest(StoneTypeVm item)
    {
        return new UpdateStoneTypeRequest(item.Title, item.EnTitle, item.Symbol);
    }
}
