using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum CustomerType
{
    [Display(Name = "مشتری خرده فروشی")]
    RetailCustomer,

    [Display(Name = "بنکدار")]
    Wholesaler,         // بنکدار

    [Display(Name = "آبشده فروش")]
    MeltedGoldDealer,   // آبشده فروش

    [Display(Name = "کارگاه دار")]
    Workshop,           // کارگاه دار

    [Display(Name = "ویترین دار/خرده فروش")]
    Retailer,            // ویترین دار / خرده فروش

    [Display(Name = "آزمایشگاه/مرکز ری گیری")]
    AssayingLab
}