using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.FinancialAccounts;

public record CashAccountRequestDto(string? Title, CashAccountType AccountType);