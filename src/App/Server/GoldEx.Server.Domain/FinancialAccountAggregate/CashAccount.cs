using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.FinancialAccountAggregate;

public class CashAccount : EntityBase
{
    public static CashAccount Create(string? title, CashAccountType accountType)
    {
        return new CashAccount
        {
            Title = title,
            AccountType = accountType
        };
    }

    public string? Title { get; private set; }
    public CashAccountType AccountType { get; private set; }
}