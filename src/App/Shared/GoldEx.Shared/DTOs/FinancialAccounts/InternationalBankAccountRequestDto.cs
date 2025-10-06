namespace GoldEx.Shared.DTOs.FinancialAccounts;

public record InternationalBankAccountRequestDto(string? SwiftBicCode, string? IbanNumber, string AccountNumber);