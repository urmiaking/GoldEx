using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum FinancialAccountType
{
    [Display(Name = "حساب بانکی داخلی")]
    LocalBankAccount = 0,

    [Display(Name = "حساب بانکی خارجی")]
    InternationalBankAccount = 1,

    [Display(Name = "نقدی")]
    Cash = 2,

    [Display(Name = "طلایی")]
    Gold = 3
}