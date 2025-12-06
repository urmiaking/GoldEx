using GoldEx.Shared.Constants;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Utilities;

public static class LedgerAccountTitleBuilder
{
    public static string ForCurrencyAccount(string priceUnitTitle)
    {
        return $"{SystemLedgerAccounts.CashAccounts} - {priceUnitTitle}";
    }

    public static string ForCurrencyInternalAccount(string priceUnitTitle)
    {
        return $"{SystemLedgerAccounts.InternalCashAccounts} - {priceUnitTitle}";
    }

    public static string ForCoinAccount(string coinTitle)
    {
        return $"{SystemLedgerAccounts.CoinInventory} - {coinTitle}";
    }

    public static string ForFinancialAccount(
        FinancialAccountType accountType,
        string? brokerName,
        string? bankIdentifier,
        string? priceUnitTitle)
    {
        return accountType switch
        {
            FinancialAccountType.Cash when string.IsNullOrEmpty(priceUnitTitle)
                => throw new ArgumentException("Price unit title is required for cash accounts.", nameof(priceUnitTitle)),

            // Bank accounts require at least broker or identifier to make a readable title
            FinancialAccountType.LocalBankAccount or FinancialAccountType.InternationalBankAccount
                when string.IsNullOrEmpty(brokerName) && string.IsNullOrEmpty(bankIdentifier)
                => throw new ArgumentException("At least one bank identifier or broker name must be provided.", nameof(accountType)),

            _ => accountType switch
            {
                FinancialAccountType.LocalBankAccount or FinancialAccountType.InternationalBankAccount
                    => bankIdentifier is not null
                        ? $"{SystemLedgerAccounts.Banks} - {brokerName} ({bankIdentifier})"
                        : $"{SystemLedgerAccounts.Banks} - {brokerName}", // No parentheses if no identifier

                FinancialAccountType.Cash when string.IsNullOrEmpty(brokerName)
                    => ForCurrencyAccount(priceUnitTitle!),

                FinancialAccountType.Cash
                    => $"{SystemLedgerAccounts.DepositsWithOthers} - {brokerName} ({priceUnitTitle})",

                _ => throw new ArgumentOutOfRangeException(nameof(accountType))
            }
        };
    }

    public static string ForCustomer(string parentTitle, string customerName, string nationalId, string priceUnitTitle)
    {
        return $"{parentTitle} - {customerName} ({nationalId}) - {priceUnitTitle}";
    }

    public static string? BestBankIdentifier(
        string? accountNumber,
        string? cardNumber,
        string? shabaNumber,
        string? ibanNumber,
        string? swift)
    {
        return accountNumber
               ?? cardNumber
               ?? shabaNumber
               ?? ibanNumber
               ?? swift
               ?? null;
    }
}