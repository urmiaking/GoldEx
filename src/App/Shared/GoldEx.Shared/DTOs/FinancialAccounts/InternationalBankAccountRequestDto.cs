namespace GoldEx.Shared.DTOs.FinancialAccounts;

public record InternationalBankAccountRequestDto(string AccountHolderName, string BankName, string SwiftBicCode, string IbanNumber, string AccountNumber);