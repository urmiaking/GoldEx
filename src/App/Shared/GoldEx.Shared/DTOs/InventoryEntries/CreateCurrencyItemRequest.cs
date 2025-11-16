namespace GoldEx.Shared.DTOs.InventoryEntries;

public record CreateCurrencyItemRequest(Guid CurrencyId, decimal Amount, decimal UnitPrice, Guid FinancialAccountId);