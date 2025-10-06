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
        $"سررسید فاکتور شماره {invoiceNumber} به ارزش {totalAmount.ToCurrencyFormat(priceUnitTitle)} " +
        $"مربوط به {customerFullName} رسیده است. " +
        $"مانده این فاکتور {totalUnpaidAmount.ToCurrencyFormat(priceUnitTitle)} می باشد";
}