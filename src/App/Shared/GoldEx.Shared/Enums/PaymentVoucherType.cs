using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum PaymentVoucherType
{
    [Display(Name = "پیش پرداخت")]
    PrepaymentToSupplier = 0,

    [Display(Name = "استرداد وجه")]
    RefundToCustomer = 1
}