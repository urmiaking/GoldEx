namespace GoldEx.Shared.DTOs.InventoryEntries;

public record InventoryEntryResponse(
    Guid Id,
    DateTime OperationDate,
    decimal ProductsAmount,
    decimal CoinsAmount,
    decimal CurrenciesAmount);