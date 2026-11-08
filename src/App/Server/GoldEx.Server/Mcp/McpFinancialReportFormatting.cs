using System.Globalization;

namespace GoldEx.Server.Mcp;

/// <summary>
/// MCP reports must not silently round stored financial or gold amounts.
/// Formatting is independent of the current server locale.
/// </summary>
public static class McpFinancialReportFormatting
{
    public static string Exact(decimal amount) =>
        amount.ToString("#,##0.############################", CultureInfo.InvariantCulture);

    public static DateTime? EndOfDay(DateTime? date) =>
        date?.Date.AddDays(1).AddTicks(-1);

    public static string BalanceStatus(decimal balance) =>
        balance > 0 ? "بدهکار" : balance < 0 ? "بستانکار" : "تسویه";
}
