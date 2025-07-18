namespace GoldEx.Shared.DTOs.BankAccounts;

public record GetInternationalBankAccountResponse(string SwiftBicCode, string IbanNumber, string AccountNumber);