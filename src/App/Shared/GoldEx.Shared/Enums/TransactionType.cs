using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum TransactionType
{
    [Display(Name = "بستانکار")]
    Credit = 0,

    [Display(Name = "بدهکار")]
    Debit = 1
}