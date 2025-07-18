namespace GoldEx.Shared.DTOs.BankAccounts;

public record InternationalBankAccountRequestDto(string SwiftBicCode, string IbanNumber, string AccountNumber);