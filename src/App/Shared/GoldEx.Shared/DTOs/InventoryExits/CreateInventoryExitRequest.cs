using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.InventoryExits;

public record CreateInventoryExitRequest(
    ExitReason ExitReason,
    DateTime ExitDate,
    string? Description,
    List<CreateProductItemExitRequest> Products,
    List<CreateCoinItemExitRequest> Coins,
    List<CreateCurrencyItemExitRequest> Currencies);