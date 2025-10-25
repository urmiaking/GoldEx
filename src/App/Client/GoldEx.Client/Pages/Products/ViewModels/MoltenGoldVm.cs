using System.ComponentModel.DataAnnotations;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Shared.DTOs.Products;

namespace GoldEx.Client.Pages.Products.ViewModels;

public class MoltenGoldVm
{
    [Display(Name = "شماره انگ")]
    public string? AssayNumber { get; set; }

    [Display(Name = "آزمایشگاه")]
    public CustomerVm? Assayer { get; set; }

    [Display(Name = "تاریخ ری گیری")]
    public DateTime? AssayDate { get; set; }

    public MoltenGoldDto ToRequest()
    {
        return new MoltenGoldDto(AssayNumber, Assayer?.Id, AssayDate);
    }

    public static MoltenGoldVm CreateFrom(GetMoltenGoldResponse? dto)
    {
        if (dto is null)
            return new MoltenGoldVm();

        return new MoltenGoldVm
        {
            AssayNumber = dto.AssayNumber,
            AssayDate = dto.AssayDate,
            Assayer = dto.Assayer != null ? CustomerVm.CreateFrom(dto.Assayer) : null
        };
    }
}