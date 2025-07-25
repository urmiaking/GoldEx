namespace GoldEx.Shared.DTOs.FinancialAccounts;

public record GetInternationalBankAccountResponse(string AccountHolderName, string BankName, string SwiftBicCode, string IbanNumber, string AccountNumber);