using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.InventoryExits;

public record InventoryExitResponse(Guid Id,
    DateTime OperationDate,
    DateTime ExitDate,
    ExitReason ExitReason,
    string? Description,
    decimal ProductsAmount,
    decimal CoinsAmount,
    decimal CurrenciesAmount);