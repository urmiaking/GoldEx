using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Dashboard;

public record DashboardOverviewResponse(
    DateOnly FromDate,
    DateOnly ToDate,
    int SellCount,
    int PurchaseCount,
    int ActiveCustomerCount,
    List<DashboardTradeSeries> Units,
    List<DashboardSettlementSummary> Settlements,
    int OverdueCount,
    int DueTodayCount,
    List<DashboardDueInvoice> DueInvoices);

public record DashboardTradeSeries(
    Guid PriceUnitId,
    string PriceUnit,
    decimal SellAmount,
    decimal PurchaseAmount,
    decimal PreviousSellAmount,
    decimal PreviousPurchaseAmount,
    List<DashboardTradePoint> Points);

public record DashboardTradePoint(DateOnly Date, decimal SellAmount, decimal PurchaseAmount);

public record DashboardSettlementSummary(string PriceUnit, decimal Received, decimal Paid, int Count);

public record DashboardDueInvoice(
    long Number,
    InvoiceType Type,
    DateOnly DueDate,
    string Customer,
    decimal Remaining,
    string PriceUnit);
