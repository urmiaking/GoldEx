using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.FinancialAccountAggregate;

public readonly record struct FinancialAccountId(Guid Value);
public class FinancialAccount : EntityBase<FinancialAccountId>
{
    public static FinancialAccount Create(
        FinancialAccountType accountType,
        PriceUnitId priceUnitId,
        CustomerId customerId,
        LocalBankAccount? localAccount = null,
        InternationalBankAccount? internationalAccount = null)
    {
        return new FinancialAccount
        {
            Id = new FinancialAccountId(Guid.NewGuid()),
            AccountType = accountType,
            PriceUnitId = priceUnitId,
            CustomerId = customerId,
            IsSystemAccount = false,
            LocalAccount = localAccount,
            InternationalAccount = internationalAccount
        };
    }

    public static FinancialAccount CreateSystemAccount(
        FinancialAccountType accountType,
        PriceUnitId priceUnitId,
        LocalBankAccount? localAccount = null,
        InternationalBankAccount? internationalAccount = null)
    {
        return new FinancialAccount
        {
            Id = new FinancialAccountId(Guid.NewGuid()),
            AccountType = accountType,
            PriceUnitId = priceUnitId,
            IsSystemAccount = true
        };
    }

    public FinancialAccountType AccountType { get; private set; }
    
    public bool IsSystemAccount { get; private set; }

    public PriceUnitId PriceUnitId { get; private set; }
    public PriceUnit? PriceUnit { get; private set; }

    public CustomerId? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public LocalBankAccount? LocalAccount { get; private set; }
    public InternationalBankAccount? InternationalAccount { get; private set; }

#pragma warning disable CS8618
    private FinancialAccount() { }
#pragma warning restore CS8618

    public FinancialAccount SetAccountType(FinancialAccountType accountType)
    {
        AccountType = accountType;
        return this;
    }

    public FinancialAccount SetPriceUnitId(PriceUnitId priceUnitId)
    {
        PriceUnitId = priceUnitId;
        return this;
    }

    public FinancialAccount SetLocalAccount(LocalBankAccount? localAccount)
    {
        LocalAccount = localAccount;
        return this;
    }

    public FinancialAccount SetInternationalAccount(InternationalBankAccount? internationalAccount)
    {
        InternationalAccount = internationalAccount;
        return this;
    }
}