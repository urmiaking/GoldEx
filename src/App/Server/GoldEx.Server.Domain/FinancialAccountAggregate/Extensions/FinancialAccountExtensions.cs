using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.FinancialAccountAggregate.Extensions;

public static class FinancialAccountExtensions
{
    /// <summary>
    /// تولید متن کامل اطلاعات حساب مالی برای نمایش
    /// </summary>
    public static string GetAccountDisplayText(this FinancialAccount? account)
    {
        if (account == null)
            return string.Empty;

        return account.AccountType switch
        {
            FinancialAccountType.LocalBankAccount =>
                string.Join(" - ", new[]
                {
                    account.AccountType.GetDisplayName(),
                    account.BrokerName,
                    account.HolderName,
                    account.LocalAccount?.AccountNumber
                }.Where(s => !string.IsNullOrEmpty(s))),

            FinancialAccountType.InternationalBankAccount =>
                string.Join(" - ", new[]
                {
                    account.AccountType.GetDisplayName(),
                    account.BrokerName,
                    account.HolderName,
                    account.InternationalAccount?.AccountNumber
                }.Where(s => !string.IsNullOrEmpty(s))),

            FinancialAccountType.Cash => account.CashAccount?.AccountType switch
            {
                CashAccountType.DepositsWithOthers =>
                    string.Join(" - ", new[]
                    {
                        account.CashAccount?.AccountType.GetDisplayName(),
                        account.CashAccount?.Title,
                        account.BrokerName,
                        account.HolderName
                    }.Where(s => !string.IsNullOrEmpty(s))),

                CashAccountType.Internal =>
                    string.Join(" - ", new[]
                    {
                        account.CashAccount?.AccountType.GetDisplayName(),
                        account.CashAccount?.Title
                    }.Where(s => !string.IsNullOrEmpty(s))),

                _ => account.AccountType.GetDisplayName()
            },

            FinancialAccountType.Gold => account.AccountType.GetDisplayName(),

            _ => "حساب نامشخص"
        };
    }

    /// <summary>
    /// تولید متن کوتاه‌شده برای حساب مالی (فقط نام صاحب حساب)
    /// </summary>
    public static string GetAccountShortText(this FinancialAccount? account)
    {
        if (account == null)
            return string.Empty;

        return account.AccountType switch
        {
            FinancialAccountType.LocalBankAccount or
            FinancialAccountType.InternationalBankAccount =>
                account.HolderName ?? account.BrokerName ?? "حساب بانکی",

            FinancialAccountType.Cash =>
                account.CashAccount?.Title ?? account.HolderName ?? "حساب نقدی",

            FinancialAccountType.Gold => "حساب طلا",

            _ => "حساب مالی"
        };
    }
}