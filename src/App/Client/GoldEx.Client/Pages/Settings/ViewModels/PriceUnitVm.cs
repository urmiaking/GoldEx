using GoldEx.Shared.DTOs.PriceUnits;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Settings.ViewModels;

public class PriceUnitVm
{
    public Guid Id { get; set; }

    public int Index { get; set; }

    [Display(Name = "عنوان")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string Title { get; set; } = default!;

    [Display(Name = "وضعیت")]
    public bool IsActive { get; set; }

    [Display(Name = "پیش فرض")]
    public bool IsDefault { get; set; }

    public bool HasIcon { get; set; }

    [Display(Name = "قیمت وابسته")]
    public Guid? PriceId { get; set; }

    [Display(Name = "قیمت وابسته")]
    public PriceVm? PriceVm { get; set; }

    [Display(Name = "لوگوی نماد")]
    public IBrowserFile? IconFile { get; set; }

    public static PriceUnitVm CreateFrom(GetPriceUnitResponse response)
    {
        return new PriceUnitVm
        {
            Id = response.Id,
            Title = response.Title,
            IsActive = response.IsActive,
            HasIcon = response.HasIcon,
            IsDefault = response.IsDefault,
            PriceId = response.PriceId,
            PriceVm = response.PriceId.HasValue && !string.IsNullOrEmpty(response.PriceTitle) ?
                new PriceVm
                {
                    Id = response.PriceId.Value,
                    Title = response.PriceTitle
                } : null
        };
    }

    public static CreatePriceUnitRequest ToCreateRequest(PriceUnitVm item, byte[]? iconContent)
    {
        return new CreatePriceUnitRequest(item.Title, iconContent, item.PriceId);
    }

    public static UpdatePriceUnitRequest ToUpdateRequest(PriceUnitVm item, byte[]? iconContent)
    {
        return new UpdatePriceUnitRequest(item.Title, iconContent, item.PriceId);
    }
}