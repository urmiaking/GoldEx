using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum CustomerType
{
    [Display(Name = "مشتری خرده فروشی")]
    RetailCustomer,

    [Display(Name = "بنکدار")]
    Wholesaler,

    [Display(Name = "آبشده فروش")]
    MeltedGoldDealer,

    [Display(Name = "کارگاه دار")]
    Workshop,

    [Display(Name = "ویترین دار/خرده فروش")]
    Retailer,

    [Display(Name = "آزمایشگاه/مرکز ری گیری")]
    AssayingLab
}