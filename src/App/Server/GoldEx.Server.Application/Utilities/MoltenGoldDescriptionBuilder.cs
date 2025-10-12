using GoldEx.Shared.Helpers;

namespace GoldEx.Server.Application.Utilities;

public static class MoltenGoldDescriptionBuilder
{
    public static string BuildMoltenEntryDebit(decimal weight, decimal fineness, string assayNumber, int batchNumber, string? assayerName)
    {
        return $"انتقال به موجودی: {weight} گرم طلای آبشده عیار {fineness} (انگ {assayNumber}) از درخواست ذوب {batchNumber} - آزمایشگاه {assayerName ?? "نامشخص"}";
    }

    public static string BuildMoltenEntryCredit(decimal weight, decimal fineness, string assayNumber, int batchNumber, decimal moltenValue, string priceUnitTitle)
    {
        return $"تسویه بهای تمام‌شده: ورود {weight} گرم طلای آبشده عیار {fineness} (انگ {assayNumber}) از درخواست ذوب شماره {batchNumber} (ارزش خالص {moltenValue.ToCurrencyFormat(priceUnitTitle)} پس از کسر ناخالصی)";
    }

    public static string BuildAssayFeeDebit(decimal feeAmount, string feePriceUnitTitle, decimal? feeExchangeRate, decimal feeBaseAmount, string priceUnitTitle, int batchNumber, string assayNumber, string? assayerName)
    {
        var equivalentPart = feeExchangeRate.HasValue ? $" (معادل {feeBaseAmount.ToCurrencyFormat(priceUnitTitle)})" : string.Empty;
        return $"ثبت هزینه ری‌گیری: {feeAmount.ToCurrencyFormat(feePriceUnitTitle)}{equivalentPart} برای درخواست ذوب {batchNumber} - انگ {assayNumber}، آزمایشگاه {assayerName ?? "نامشخص"}";
    }

    public static string BuildAssayFeeCredit(decimal feeAmount, string feePriceUnitTitle, decimal? feeExchangeRate, decimal feeBaseAmount, string priceUnitTitle, int batchNumber)
    {
        var equivalentPart = feeExchangeRate.HasValue ? $" (معادل {feeBaseAmount.ToCurrencyFormat(priceUnitTitle)})" : string.Empty;
        return $"پرداخت هزینه ری‌گیری: {feeAmount.ToCurrencyFormat(feePriceUnitTitle)}{equivalentPart} برای درخواست ذوب {batchNumber}";
    }
}