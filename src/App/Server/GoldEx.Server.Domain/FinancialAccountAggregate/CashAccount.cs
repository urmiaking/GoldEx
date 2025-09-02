using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.FinancialAccountAggregate;

public class CashAccount : EntityBase
{
    public static CashAccount Create(CashAccountType accountType)
    {
        return new CashAccount
        {
            AccountType = accountType
        };
    }

    public CashAccountType AccountType { get; private set; }
}