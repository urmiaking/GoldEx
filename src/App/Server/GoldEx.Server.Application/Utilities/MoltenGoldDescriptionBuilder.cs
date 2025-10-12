using GoldEx.Shared.Helpers;

namespace GoldEx.Server.Application.Utilities;

public static class MoltenGoldDescriptionBuilder
{
    public static string BuildMoltenEntryDebit(decimal weight, decimal fineness, string assayNumber, int batchNumber, string? assayerName)
    {
        return $"ورود به موجودی آبشده: {weight}g عیار {fineness} (انگ {assayNumber}) - درخواست {batchNumber}، آزمایشگاه {assayerName ?? "نامشخص"}";
    }

    public static string BuildMoltenEntryCredit(decimal weight, decimal fineness, string assayNumber, int batchNumber, decimal moltenValue, string priceUnitTitle)
    {
        return $"تسویه COGS: {weight}g عیار {fineness} (انگ {assayNumber}) - درخواست {batchNumber} (خالص {moltenValue.ToCurrencyFormat(priceUnitTitle)})";
    }

    public static string BuildAssayFeeDebit(decimal feeAmount, string feePriceUnitTitle, decimal? feeExchangeRate, decimal feeBaseAmount, string priceUnitTitle, int batchNumber, string assayNumber, string? assayerName)
    {
        var equivalentPart = feeExchangeRate.HasValue ? $" (معادل {feeBaseAmount.ToCurrencyFormat(priceUnitTitle)})" : string.Empty;
        return $"هزینه ری‌گیری: {feeAmount.ToCurrencyFormat(feePriceUnitTitle)}{equivalentPart} - درخواست {batchNumber}، انگ {assayNumber}، آزمایشگاه {assayerName ?? "نامشخص"}";
    }

    public static string BuildAssayFeeCredit(decimal feeAmount, string feePriceUnitTitle, decimal? feeExchangeRate, decimal feeBaseAmount, string priceUnitTitle, int batchNumber)
    {
        var equivalentPart = feeExchangeRate.HasValue ? $" (معادل {feeBaseAmount.ToCurrencyFormat(priceUnitTitle)})" : string.Empty;
        return $"پرداخت هزینه ری‌گیری: {feeAmount.ToCurrencyFormat(feePriceUnitTitle)}{equivalentPart} - درخواست {batchNumber}";
    }
}