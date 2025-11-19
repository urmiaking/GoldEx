namespace GoldEx.Shared.DTOs.InventoryEntries;

public record SkippedRowResponse(int RowIndex, List<string?> RowValues, string Reason);