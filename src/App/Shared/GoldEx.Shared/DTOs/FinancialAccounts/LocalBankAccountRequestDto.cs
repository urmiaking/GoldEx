namespace GoldEx.Shared.DTOs.FinancialAccounts;

public record LocalBankAccountRequestDto(string? CardNumber, string? ShabaNumber, string? AccountNumber);