namespace GoldEx.Shared.DTOs.InventoryEntries;

public record CreateInventoryEntryRequest(
    List<CreateProductItemEntryRequest> Products,
    List<CreateCurrencyItemEntryRequest> Currencies,
    List<CreateCoinItemEntryRequest> Coins);