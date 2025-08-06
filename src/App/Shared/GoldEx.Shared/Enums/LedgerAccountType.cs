using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum LedgerAccountType
{
    [Display(Name = "دارایی")]
    Asset = 0,      // دارایی

    [Display(Name = "بدهی")]
    Liability = 1,  // بدهی

    [Display(Name = "حقوق صاحبان سهام")]
    Equity = 2,     // حقوق صاحبان سهام

    [Display(Name = "درآمد")]
    Revenue = 3,    // درآمد

    [Display(Name = "هزینه")]
    Expense = 4     // هزینه
}