using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.BankAccountAggregate;

public readonly record struct BankAccountId(Guid Value);
public class BankAccount : EntityBase<BankAccountId>
{
    public static BankAccount Create(
        BankAccountType accountType,
        string accountHolderName,
        string bankName,
        PriceUnitId priceUnitId,
        CustomerId customerId,
        LocalBankAccount? localAccount = null,
        InternationalBankAccount? internationalAccount = null)
    {
        return new BankAccount
        {
            Id = new BankAccountId(Guid.NewGuid()),
            AccountType = accountType,
            AccountHolderName = accountHolderName,
            BankName = bankName,
            PriceUnitId = priceUnitId,
            CustomerId = customerId,
            LocalAccount = localAccount,
            InternationalAccount = internationalAccount
        };
    }

    public BankAccountType AccountType { get; private set; }
    public string AccountHolderName { get; private set; }
    public string BankName { get; private set; }

    public PriceUnitId PriceUnitId { get; private set; }
    public PriceUnit? PriceUnit { get; private set; }

    public CustomerId CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public LocalBankAccount? LocalAccount { get; private set; }
    public InternationalBankAccount? InternationalAccount { get; private set; }

#pragma warning disable CS8618
    private BankAccount() { }
#pragma warning restore CS8618

    public BankAccount SetAccountType(BankAccountType accountType)
    {
        AccountType = accountType;
        return this;
    }

    public BankAccount SetAccountHolderName(string accountHolderName)
    {
        AccountHolderName = accountHolderName;
        return this;
    }

    public BankAccount SetBankName(string bankName)
    {
        BankName = bankName;
        return this;
    }

    public BankAccount SetPriceUnitId(PriceUnitId priceUnitId)
    {
        PriceUnitId = priceUnitId;
        return this;
    }

    public BankAccount SetLocalAccount(LocalBankAccount? localAccount)
    {
        LocalAccount = localAccount;
        return this;
    }

    public BankAccount SetInternationalAccount(InternationalBankAccount? internationalAccount)
    {
        InternationalAccount = internationalAccount;
        return this;
    }
}