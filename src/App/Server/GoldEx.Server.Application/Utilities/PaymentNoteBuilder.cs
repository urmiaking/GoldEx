using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Utilities;

public static class PaymentNoteBuilder
{
    public static string BuildForTransfer(string customerName,
        long invoiceNumber,
        InvoiceType invoiceType) =>
        $"کسر از بدهی بابت پرداخت به {customerName} (فاکتور {invoiceType.GetDisplayName()} شماره {invoiceNumber})";
}