using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum BankAccountType
{
    [Display(Name = "حساب بانکی محلی")]
    Local = 0,

    [Display(Name = "حساب بانکی بین المللی")]
    International = 1
}