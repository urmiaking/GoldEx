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
            PaymentType.InternalCash => "پرداخت نقدی",
            PaymentType.UsedGoldInventory => $"پرداخت {payment.Amount.ToCurrencyReportFormat(payment.PriceUnit?.Title)} طلای شکسته عیار {payment.GoldFineness:G29}" + (payment.Amount != payment.FinalAmount ? $" (معادل {payment.FinalAmount.ToCurrencyReportFormat(payment.PriceUnit?.Title)} طلای 18 عیار)" : ""),
            PaymentType.MoltenGoldInventory => $"پرداخت {payment.Amount.ToCurrencyReportFormat(payment.PriceUnit?.Title)} طلای آب شده عیار {payment.GoldFineness:G29}" + (payment.Amount != payment.FinalAmount ? $" (معادل {payment.FinalAmount.ToCurrencyReportFormat(payment.PriceUnit?.Title)} طلای 18 عیار)" : ""),
            PaymentType.CustomerTransfer => $"حواله به {payment.LedgerAccount?.Customer?.FullName}",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}