namespace GoldEx.Shared.DTOs.InventoryEntries;

public record ProcessExcelResponse(
    int TotalRows,
    int MappedRows,
    int SkippedRows,
    List<SkippedRowResponse> SkippedRowDetails,
    List<GetProductItemEntryResponse> Items);