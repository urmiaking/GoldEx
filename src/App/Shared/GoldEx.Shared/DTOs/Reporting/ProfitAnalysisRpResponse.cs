namespace GoldEx.Shared.DTOs.Reporting;

public record ProfitAnalysisRpResponse(
    List<ProfitAnalysisSummary> Summaries,
    List<ProfitAnalysisRow> Items,
    int TotalCount,
    int CalculatedCount,
    int IncompleteCount);

public record ProfitAnalysisSummary(
    Guid PriceUnitId,
    string PriceUnit,
    int Count,
    decimal NetSales,
    decimal PurchaseCost,
    decimal GrossProfit,
    decimal GoldPriceEffect,
    decimal ExchangeRateEffect,
    decimal TradingMargin);

public record ProfitAnalysisRow(
    Guid InvoiceId,
    long InvoiceNumber,
    DateOnly InvoiceDate,
    string CustomerName,
    string ProductName,
    string Barcode,
    decimal WeightEquivalent,
    Guid? PurchaseInvoiceId,
    long? PurchaseInvoiceNumber,
    DateOnly? PurchaseDate,
    Guid PriceUnitId,
    string PriceUnit,
    decimal? NetSales,
    decimal? PurchaseCost,
    decimal? GrossProfit,
    decimal? GoldPriceEffect,
    decimal? ExchangeRateEffect,
    decimal? TradingMargin,
    string? IncompleteReason);
