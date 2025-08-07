using GoldEx.Sdk.Common.Extensions;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;

namespace GoldEx.Server.Application.Utilities;

public static class TransactionDescriptionBuilder
{
    #region Sale Invoice Descriptions (شرح‌های فاکتور فروش)

    /// <summary>
    /// شرح اصلی برای بدهکار کردن مشتری در فاکتور فروش
    /// </summary>
    public static string ForSaleReceivable(Invoice invoice, Customer customer)
    {
        return $"ثبت بدهی {customer.FullName} بابت فاکتور فروش شماره {invoice.InvoiceNumber}";
    }

    /// <summary>
    /// شرح برای بستانکار کردن حساب درآمد فروش
    /// </summary>
    public static string ForSaleRevenue(Invoice invoice)
    {
        return $"ثبت درآمد فروش طبق فاکتور شماره {invoice.InvoiceNumber}";
    }

    /// <summary>
    /// شرح برای ثبت تخفیف اعمال شده در فاکتور فروش
    /// </summary>
    public static string ForSaleDiscount(Invoice invoice, IEnumerable<string?> descriptions)
    {
        var desc = $"ثبت تخفیف فروش مربوط به فاکتور شماره {invoice.InvoiceNumber}";
        foreach (var description in descriptions)
            if (!string.IsNullOrWhiteSpace(description)) 
                desc += $" (بابت: {description})";

        return desc;
    }

    /// <summary>
    /// شرح برای ثبت درآمد حاصل از هزینه‌های اضافی در فاکتور فروش
    /// </summary>
    public static string ForSaleExtraCharges(Invoice invoice)
    {
        return $"ثبت درآمد هزینه‌های اضافی فاکتور شماره {invoice.InvoiceNumber}";
    }

    #endregion

    #region Purchase Invoice Descriptions (شرح‌های فاکتور خرید)

    /// <summary>
    /// شرح برای ثبت ورود کالا به انبار در فاکتور خرید
    /// </summary>
    public static string ForPurchaseInventoryEntry(Invoice invoice)
    {
        return $"ثبت ورود کالا به انبار طبق فاکتور خرید شماره {invoice.InvoiceNumber}";
    }

    /// <summary>
    /// شرح برای ثبت بدهی به تامین‌کننده در فاکتور خرید
    /// </summary>
    public static string ForPurchasePayable(Invoice invoice, Customer supplier)
    {
        return $"ثبت بدهی به {supplier.FullName} بابت فاکتور خرید شماره {invoice.InvoiceNumber}";
    }

    /// <summary>
    /// شرح برای ثبت تخفیف دریافت شده در فاکتور خرید
    /// </summary>
    public static string ForPurchaseDiscount(Invoice invoice, Customer supplier, IEnumerable<string?> descriptions)
    {
        var desc = $"ثبت تخفیف دریافتی از {supplier.FullName} بابت فاکتور شماره {invoice.InvoiceNumber}";

        foreach (var description in descriptions)
            if (!string.IsNullOrWhiteSpace(description))
                desc += $" (بابت: {description})";

        return desc;
    }

    #endregion

    #region COGS Descriptions (شرح‌های بهای تمام شده)

    /// <summary>
    /// شرح برای بدهکار کردن حساب بهای تمام شده
    /// </summary>
    public static string ForCostOfGoodsSold(Invoice invoice)
    {
        return $"ثبت بهای تمام شده کالاهای فاکتور فروش شماره {invoice.InvoiceNumber}";
    }

    /// <summary>
    /// شرح برای بستانکار کردن انبار بابت خروج کالا
    /// </summary>
    public static string ForInventoryExit(Invoice invoice)
    {
        return $"خروج کالا از انبار بابت فاکتور فروش شماره {invoice.InvoiceNumber}";
    }

    #endregion

    #region Payment Descriptions (شرح‌های پرداخت)

    public static string ForInvoicePaymentReceived(Invoice invoice, InvoicePayment payment)
    {
        var desc = $"دریافت وجه بابت فاکتور فروش شماره {invoice.InvoiceNumber}";
        if (!string.IsNullOrWhiteSpace(payment.ReferenceNumber))
            desc += $" (شماره پیگیری: {payment.ReferenceNumber})";
        return desc;
    }

    public static string ForInvoicePaymentMade(Invoice invoice, InvoicePayment payment)
    {
        var desc = $"پرداخت وجه بابت فاکتور خرید شماره {invoice.InvoiceNumber}";
        if (!string.IsNullOrWhiteSpace(payment.ReferenceNumber))
            desc += $" (شماره پیگیری: {payment.ReferenceNumber})";
        return desc;
    }

    public static string ForPaymentVoucher(PaymentVoucher voucher, Invoice invoice)
    {
        return $"اعمال سند پرداخت شماره {voucher.VoucherNumber} بابت فاکتور خرید شماره {invoice.InvoiceNumber}";
    }

    #endregion

    #region Payment Voucher Descriptions (شرح‌های سند پیش‌پرداخت)

    /// <summary>
    /// شرح برای بدهکار کردن حساب "پیش‌پرداخت"
    /// </summary>
    public static string ForPrepaymentAsset(PaymentVoucher voucher, Customer supplier)
    {
        return $"ثبت طلب از {supplier.FullName} بابت پیش پرداخت شماره {voucher.VoucherNumber}";
    }

    /// <summary>
    /// شرح برای بستانکار کردن حساب بانک/صندوق بابت پیش‌پرداخت
    /// </summary>
    public static string ForPrepaymentCashExit(PaymentVoucher voucher, FinancialAccount sourceAccount)
    {
        return $"خروج وجه از حساب '{sourceAccount.AccountType.GetDisplayName()}' بابت پیش پرداخت شماره {voucher.VoucherNumber}";
    }

    #endregion
}