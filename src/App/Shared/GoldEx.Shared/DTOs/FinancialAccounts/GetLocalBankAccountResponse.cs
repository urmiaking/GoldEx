namespace GoldEx.Shared.DTOs.FinancialAccounts;

public record GetLocalBankAccountResponse(string AccountHolderName, string BankName, string CardNumber, string ShabaNumber, string AccountNumber);