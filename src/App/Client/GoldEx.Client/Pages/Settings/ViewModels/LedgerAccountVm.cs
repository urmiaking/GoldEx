using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.LedgerAccounts;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.Settings.ViewModels;

public class LedgerAccountVm
{
    public Guid? Id { get; set; }

    [Display(Name = "عنوان")]
    [Required(ErrorMessage = "عنوان الزامی است.")]
    [StringLength(100, ErrorMessage = "عنوان نمی‌تواند بیشتر از 100 کاراکتر باشد.")]
    public string? Title { get; set; }

    [Display(Name = "نوع حساب")]
    [Required(ErrorMessage = "نوع حساب الزامی است.")]
    public LedgerAccountType? AccountType { get; set; }

    public Guid? ParentAccountId { get; set; }

    public static LedgerAccountRequestDto ToRequest(LedgerAccountVm model)
    {
        if (model.AccountType == null)
            throw new InvalidOperationException();

        return new LedgerAccountRequestDto(
            Id: model.Id,
            Title: model.Title ?? string.Empty,
            AccountType: model.AccountType ?? LedgerAccountType.Asset,
            ParentAccountId: model.ParentAccountId);
    }

    public static LedgerAccountVm CreateFrom(GetLedgerAccountResponse selectedItem)
    {
        return new LedgerAccountVm
        {
            Id = selectedItem.Id,
            Title = selectedItem.Title,
            AccountType = selectedItem.AccountType,
            ParentAccountId = selectedItem.ParentAccount?.Id
        };
    }
}