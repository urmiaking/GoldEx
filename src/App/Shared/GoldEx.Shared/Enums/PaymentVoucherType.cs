using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum PaymentVoucherType
{
    [Display(Name = "پیش پرداخت")]
    PrepaymentToSupplier = 0,

    [Display(Name = "استرداد وجه")]
    RefundToCustomer = 1,

    [Display(Name = "پرداخت هزینه خدمات")]
    ServiceFeePayment,

    [Display(Name = "پرداخت وام/تنخواه به همکار")]
    PartnerLoan,

    [Display(Name = "برداشت مالک")]
    OwnerDraw
}