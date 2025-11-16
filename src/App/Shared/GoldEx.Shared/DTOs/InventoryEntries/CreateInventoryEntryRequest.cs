namespace GoldEx.Shared.DTOs.InventoryEntries;

public record CreateInventoryEntryRequest(
    List<CreateProductItemRequest> Products,
    List<CreateCurrencyItemRequest> Currencies,
    List<CreateCoinItemRequest> Coins);