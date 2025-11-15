namespace GoldEx.Shared.DTOs.InventoryEntries;

public record CreateInventoryStockRequest(
    CreateCoinItemRequest? CoinItem,
    CreateCurrencyItemRequest? CurrencyItem,
    CreateProductItemRequest? ProductItem,
    decimal Amount,
    decimal UnitPrice);