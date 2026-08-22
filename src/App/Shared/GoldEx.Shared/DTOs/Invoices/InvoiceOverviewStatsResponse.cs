namespace GoldEx.Shared.DTOs.Invoices;

public record InvoiceOverviewStatsResponse(
    int TotalInvoicesCount,
    int SellCount,
    int PurchaseCount,
    int PaidCount,
    int DebtCount,
    int OverdueCount,
    decimal AverageInvoiceValue,
    List<InvoicePriceUnitSummaryDto> RemainingSummaries,
    List<InvoicePriceUnitSummaryDto> TodaySellSummaries,
    List<InvoicePriceUnitSummaryDto> TodayPurchaseSummaries
);

public record InvoicePriceUnitSummaryDto(
    string PriceUnit,
    decimal Amount,
    int Count,
    string Subtitle
);
