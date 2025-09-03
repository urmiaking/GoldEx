using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.FinancialAccountAggregate;

public readonly record struct FinancialAccountId(Guid Value);
public class FinancialAccount : EntityBase<FinancialAccountId>
{
    public static FinancialAccount CreateCustomerAccount(
        string? holderName,
        string? brokerName,
        FinancialAccountType accountType,
        PriceUnitId priceUnitId,
        CustomerId? customerId,
        LocalBankAccount? localAccount = null,
        InternationalBankAccount? internationalAccount = null,
        CashAccount? cashAccount = null)
    {
        if (cashAccount?.AccountType is CashAccountType.DepositsWithOthers)
        {
            ArgumentException.ThrowIfNullOrEmpty(holderName, nameof(holderName));
            ArgumentException.ThrowIfNullOrEmpty(brokerName, nameof(brokerName));
        }

        return new FinancialAccount
        {
            Id = new FinancialAccountId(Guid.NewGuid()),
            AccountType = accountType,
            PriceUnitId = priceUnitId,
            CustomerId = customerId,
            HolderName = holderName,
            BrokerName = brokerName,
            IsSystemAccount = false,
            LocalAccount = localAccount,
            InternationalAccount = internationalAccount,
            CashAccount = cashAccount
        };
    }

    public static FinancialAccount CreateSystemAccount(
        string? holderName,
        string? brokerName,
        FinancialAccountType accountType,
        PriceUnitId priceUnitId,
        LedgerAccountId ledgerAccountId,
        LocalBankAccount? localAccount = null,
        InternationalBankAccount? internationalAccount = null,
        CashAccount? cashAccount = null)
    {
        if (cashAccount?.AccountType is CashAccountType.DepositsWithOthers)
        {
            ArgumentException.ThrowIfNullOrEmpty(holderName, nameof(holderName));
            ArgumentException.ThrowIfNullOrEmpty(brokerName, nameof(brokerName));
        }

        return new FinancialAccount
        {
            Id = new FinancialAccountId(Guid.NewGuid()),
            LedgerAccountId = ledgerAccountId,
            AccountType = accountType,
            HolderName = holderName,
            BrokerName = brokerName,
            PriceUnitId = priceUnitId,
            IsSystemAccount = true,
            LocalAccount = localAccount,
            InternationalAccount = internationalAccount,
            CashAccount = cashAccount
        };
    }

    public FinancialAccountType AccountType { get; private set; }

    public string? HolderName { get; private set; }
    public string? BrokerName { get; private set; }

    public bool IsSystemAccount { get; private set; }

    public PriceUnitId PriceUnitId { get; private set; }
    public PriceUnit? PriceUnit { get; private set; }

    public CustomerId? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public LedgerAccountId? LedgerAccountId { get; private set; }
    public LedgerAccount? LedgerAccount { get; private set; }

    public LocalBankAccount? LocalAccount { get; private set; }
    public InternationalBankAccount? InternationalAccount { get; private set; }
    public CashAccount? CashAccount { get; private set; }

#pragma warning disable CS8618
    private FinancialAccount() { }
#pragma warning restore CS8618

    public FinancialAccount SetAccountType(FinancialAccountType accountType)
    {
        AccountType = accountType;
        return this;
    }

    public FinancialAccount SetHolderName(string? holderName)
    {
        HolderName = holderName;
        return this;
    }

    public FinancialAccount SetBrokerName(string? brokerName)
    {
        BrokerName = brokerName;
        return this;
    }

    public FinancialAccount SetPriceUnitId(PriceUnitId priceUnitId)
    {
        PriceUnitId = priceUnitId;
        return this;
    }

    public FinancialAccount SetLocalAccount(LocalBankAccount? localAccount)
    {
        if (AccountType != FinancialAccountType.LocalBankAccount)
            throw new InvalidOperationException("Cannot set local account for non-local bank account type.");

        LocalAccount = localAccount;
        InternationalAccount = null;
        CashAccount = null;

        return this;
    }

    public FinancialAccount SetInternationalAccount(InternationalBankAccount? internationalAccount)
    {
        if (AccountType != FinancialAccountType.InternationalBankAccount)
            throw new InvalidOperationException("Cannot set international account for non-international bank account type.");

        InternationalAccount = internationalAccount;
        LocalAccount = null;
        CashAccount = null;

        return this;
    }

    public FinancialAccount SetCashAccount(CashAccount? cashAccount)
    {
        if (AccountType != FinancialAccountType.Cash)
            throw new InvalidOperationException("Cannot set cash account for non-cash account type.");

        CashAccount = cashAccount;
        LocalAccount = null;
        InternationalAccount = null;

        return this;
    }

    public FinancialAccount SetLedgerAccount(LedgerAccountId? ledgerAccountId)
    {
        if (!IsSystemAccount && ledgerAccountId.HasValue)
            throw new InvalidOperationException("Cannot link a customer's financial account to a ledger account.");

        LedgerAccountId = ledgerAccountId;
        return this;
    }
}