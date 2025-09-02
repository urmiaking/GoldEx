using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum CashAccountType
{
    [Display(Name = "صندوق داخلی")]
    Internal = 0,

    [Display(Name = "سپرده نزد دیگران")]
    DepositsWithOwners = 1
}