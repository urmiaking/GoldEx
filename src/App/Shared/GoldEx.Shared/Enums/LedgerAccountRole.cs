using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum LedgerAccountRole
{
    [Display(Name = "پرداختنی")]
    Payable = 0,

    [Display(Name = "دریافتنی")]
    Receivable = 1
}