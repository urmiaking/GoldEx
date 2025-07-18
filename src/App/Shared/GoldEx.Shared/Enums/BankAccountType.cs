using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum BankAccountType
{
    [Display(Name = "داخلی")]
    Local = 0,

    [Display(Name = "خارجی")]
    International = 1
}