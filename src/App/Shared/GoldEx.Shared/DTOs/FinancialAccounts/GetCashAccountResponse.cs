using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.FinancialAccounts;

public record GetCashAccountResponse(string? Title, CashAccountType AccountType);