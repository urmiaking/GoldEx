using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum PaymentType
{
    [Display(Name = "حساب های داخلی")]
    InternalCash = 0,

    [Display(Name = "طلای شکسته")]
    UsedGoldInventory = 1,

    [Display(Name = "طلای آبشده")]
    MoltenGoldInventory = 2,

    [Display(Name = "حواله کردِ مشتری")]
    CustomerTransfer = 3
}