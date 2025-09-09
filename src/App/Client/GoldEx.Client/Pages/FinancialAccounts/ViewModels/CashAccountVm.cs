using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.FinancialAccounts.ViewModels;

public class CashAccountVm
{
    [Display(Name = "عنوان")]
    public string? Title { get; set; }

    [Display(Name = "نوع حساب نقدی")]
    public CashAccountType AccountType { get; set; }
}