using GoldEx.Shared.Helpers;

namespace GoldEx.Server.Application.Utilities;

public static class NotificationMessageBuilder
{
    public static string BuildTitle() => "سررسید فاکتور";

    public static string BuildMessage(long invoiceNumber,
        string customerFullName,
        decimal totalAmount,
        decimal totalUnpaidAmount,
        string priceUnitTitle) =>
        $"موعد سررسید فاکتور شماره {invoiceNumber} به نام {customerFullName} " +
        $"به ارزش {totalAmount.ToCurrencyFormat(priceUnitTitle)} فرا رسیده است. " +
        $"مانده فاکتور مذکور {totalUnpaidAmount.ToCurrencyFormat(priceUnitTitle)} می باشد";
}