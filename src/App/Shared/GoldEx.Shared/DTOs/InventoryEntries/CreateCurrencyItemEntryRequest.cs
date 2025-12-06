namespace GoldEx.Shared.DTOs.InventoryEntries;

public record CreateCurrencyItemEntryRequest(Guid CurrencyId, decimal Amount, decimal UnitPrice, Guid FinancialAccountId);