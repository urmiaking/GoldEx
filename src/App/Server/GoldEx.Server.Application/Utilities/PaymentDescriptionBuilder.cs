using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;

namespace GoldEx.Server.Application.Utilities;

public static class PaymentDescriptionBuilder
{
    public static string Build(InvoicePayment payment)
    {
        return payment.PaymentType switch
        {
            PaymentType.InternalCash => GetInternalCashTitle(payment),
            PaymentType.UsedGoldInventory => GetGoldPaymentTitle(PaymentType.UsedGoldInventory, payment),
            PaymentType.MoltenGoldInventory => GetGoldPaymentTitle(PaymentType.MoltenGoldInventory, payment),
            PaymentType.CustomerTransfer => $"حواله به {payment.LedgerAccount?.Customer?.FullName}",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static string GetGoldPaymentTitle(PaymentType paymentType, InvoicePayment payment)
    {
        if (paymentType is PaymentType.UsedGoldInventory)
        {
            return
                $"پرداخت {payment.Amount.ToCurrencyReportFormat(payment.PriceUnit?.Title)} طلای شکسته عیار {payment.GoldFineness:G29}" +
                (payment.Amount != payment.FinalAmount
                    ? $" (معادل {payment.FinalAmount.ToCurrencyReportFormat(payment.PriceUnit?.Title)} طلای 18 عیار)"
                    : "");
        }

        if (paymentType is PaymentType.MoltenGoldInventory)
        {
            return
                $"پرداخت {payment.Amount.ToCurrencyReportFormat(payment.PriceUnit?.Title)} طلای آب شده عیار {payment.GoldFineness:G29}" +
                (payment.Amount != payment.FinalAmount
                    ? $" (معادل {payment.FinalAmount.ToCurrencyReportFormat(payment.PriceUnit?.Title)} طلای 18 عیار)"
                    : "");
        }

        throw new ArgumentOutOfRangeException(nameof(paymentType), paymentType, null);
    }

    private static string GetInternalCashTitle(InvoicePayment payment)
    {
        var text = "پرداخت نقدی";

        if (payment.PriceUnitId != payment.Invoice?.PriceUnitId)
        {
            text += $" {payment.PriceUnit?.Title} با نرخ تبدیل {payment.ExchangeRate?.ToCurrencyReportFormat(payment.Invoice?.PriceUnit?.Title)}";
        }

        return text;
    }
}