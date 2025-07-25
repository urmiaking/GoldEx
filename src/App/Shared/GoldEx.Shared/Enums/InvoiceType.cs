using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum InvoiceType
{
    [Display(Name = "خرید")]
    Purchase,

    [Display(Name = "فروش")]
    Sell
}