namespace GoldEx.Shared.DTOs.FinancialAccounts;

public record GetInternationalBankAccountResponse(string SwiftBicCode, string IbanNumber, string AccountNumber);