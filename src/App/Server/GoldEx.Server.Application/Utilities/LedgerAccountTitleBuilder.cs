using GoldEx.Shared.Constants;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Utilities;

public static class LedgerAccountTitleBuilder
{
    public static string ForCurrencyAccount(string priceUnitTitle)
    {
        return $"{SystemLedgerAccounts.CashAccounts} - {priceUnitTitle}";
    }

    public static string ForCoinAccount(string coinTitle)
    {
        return $"{SystemLedgerAccounts.CoinInventory} - {coinTitle}";
    }

    public static string ForFinancialAccount(FinancialAccountType accountType, string? bankName, string? accountNumber, string? priceUnitTitle)
    {
        if (accountType is FinancialAccountType.Cash && string.IsNullOrEmpty(priceUnitTitle))
            throw new ArgumentException("Price unit title must be provided for cash accounts.", nameof(priceUnitTitle));
        if (string.IsNullOrEmpty(bankName) || string.IsNullOrEmpty(accountNumber))
            throw new ArgumentException("Bank name and account number must be provided for bank accounts.", nameof(accountType));

        return accountType switch
        {
            FinancialAccountType.LocalBankAccount or FinancialAccountType.InternationalBankAccount =>
                $"{SystemLedgerAccounts.Banks} - {bankName} ({accountNumber})",
            FinancialAccountType.Cash => ForCurrencyAccount(priceUnitTitle!),
            _ => throw new ArgumentOutOfRangeException(nameof(accountType), "Unknown financial account type")
        };
    }
}