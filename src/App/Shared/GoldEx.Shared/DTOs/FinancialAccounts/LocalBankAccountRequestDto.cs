namespace GoldEx.Shared.DTOs.FinancialAccounts;

public record LocalBankAccountRequestDto(string AccountHolderName, string BankName, string CardNumber, string ShabaNumber, string AccountNumber);