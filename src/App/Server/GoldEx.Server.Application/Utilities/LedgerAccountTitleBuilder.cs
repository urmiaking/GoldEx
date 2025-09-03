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

    public static string ForFinancialAccount(FinancialAccountType accountType, string? brokerName, string? accountNumber, string? priceUnitTitle)
    {
        return accountType switch
        {
            FinancialAccountType.Cash when string.IsNullOrEmpty(priceUnitTitle) => throw new ArgumentException(
                "Price unit title must be provided for cash accounts.", nameof(priceUnitTitle)),

            FinancialAccountType.LocalBankAccount or FinancialAccountType.InternationalBankAccount when
                string.IsNullOrEmpty(brokerName) || string.IsNullOrEmpty(accountNumber) => throw new ArgumentException(
                    "Broker name and account number must both be provided for bank accounts.", nameof(accountType)),

            _ => accountType switch
            {
                FinancialAccountType.LocalBankAccount or FinancialAccountType.InternationalBankAccount =>
                    $"{SystemLedgerAccounts.Banks} - {brokerName} ({accountNumber})",
                FinancialAccountType.Cash when string.IsNullOrEmpty(brokerName)
                    => ForCurrencyAccount(priceUnitTitle!),
                FinancialAccountType.Cash => $"{SystemLedgerAccounts.DepositsWithOthers} - {brokerName} ({priceUnitTitle})",
                _ => throw new ArgumentOutOfRangeException(nameof(accountType), "Unknown financial account type")
            }
        };
    }
}