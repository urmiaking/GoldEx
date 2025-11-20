namespace GoldEx.Server.Infrastructure.Models.Spreadsheets;

public record SkippedRowInfo(int RowIndex, List<string?> RowValues, string Reason);