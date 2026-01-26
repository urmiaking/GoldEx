namespace GoldEx.Shared.DTOs.InventoryExits;

public record CreateCurrencyItemExitRequest(Guid Id, decimal Quantity, Guid FinancialAccountId);