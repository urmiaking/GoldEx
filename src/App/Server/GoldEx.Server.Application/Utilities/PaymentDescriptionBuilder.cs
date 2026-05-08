using GoldEx.Sdk.Common.Extensions;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;

namespace GoldEx.Server.Application.Utilities;

public static class PaymentDescriptionBuilder
{
    public static string Build(InvoicePayment payment, bool includeAccountDetails = false)
    {
        return payment.PaymentType switch
        {
            PaymentType.InternalCash => GetInternalCashTitle(payment, includeAccountDetails),
            PaymentType.UsedGoldInventory => GetGoldPaymentTitle(PaymentType.UsedGoldInventory, payment),
            PaymentType.MoltenGoldInventory => GetGoldPaymentTitle(PaymentType.MoltenGoldInventory, payment),
            PaymentType.CustomerTransfer => GetCustomerTransferTitle(payment),
            PaymentType.TransferedPayment => GetTransferedPaymentTitle(payment),
            PaymentType.Check => GetCheckPaymentTitle(payment),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static string GetCheckPaymentTitle(InvoicePayment payment)
    {
        var check = payment.CheckPayment;

        if (check is null)
            return "پرداخت با چک";

        var parts = new List<string>
        {
            "پرداخت با چک"
        };

        // شماره یا صیادی
        if (!string.IsNullOrWhiteSpace(check.Number))
            parts.Add($"شماره {check.Number}");
        else if (!string.IsNullOrWhiteSpace(check.SayadiCode))
            parts.Add($"صیادی {check.SayadiCode}");

        // سررسید
        parts.Add($"سررسید {check.DueDate:yyyy/MM/dd}");

        return string.Join(" - ", parts);
    }

    private static string GetCustomerTransferTitle(InvoicePayment payment)
    {
        var text = $"حواله مشتری؛ پرداخت توسط {payment.LedgerAccount?.Customer?.FullName}";

        if (payment.TargetInvoice != null) 
            text += $" بابت فاکتور {payment.TargetInvoice?.InvoiceType.GetDisplayName()} شماره {payment.TargetInvoice?.InvoiceNumber}";

        return text;
    }

    private static string GetTransferedPaymentTitle(InvoicePayment payment) => payment.Note ?? "حواله";

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

    private static string GetInternalCashTitle(InvoicePayment payment, bool includeAccountDetails)
    {
        var text = "پرداخت نقدی";

        if (includeAccountDetails)
        {
            switch (payment.PaymentSide)
            {
                case PaymentSide.Pay:
                    text += " از حساب";
                    break;
                case PaymentSide.Receive:
                    text += " به حساب";
                    break;
            }

            text += $" {payment.SourceFinancialAccount?.GetAccountTypeText()}";
        }

        var exchangeRate = payment.ExchangeRate < 1 ? Math.Round(1 / payment.ExchangeRate.Value, 2) : payment.ExchangeRate;

        var priceUnit = payment.ExchangeRate < 1 ? payment.PriceUnit : payment.Invoice?.PriceUnit;

        if (payment.PriceUnitId != payment.Invoice?.PriceUnitId)
        {
            text += $" {payment.PriceUnit?.Title} با نرخ تبدیل {exchangeRate?.ToCurrencyReportFormat(priceUnit?.Title)}";
        }

        return text;
    }
}