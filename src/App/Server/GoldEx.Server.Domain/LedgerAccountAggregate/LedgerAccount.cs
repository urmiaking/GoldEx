using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.LedgerAccountAggregate;

public readonly record struct LedgerAccountId(Guid Value);
public class LedgerAccount : EntityBase<LedgerAccountId>
{
    public string Title { get; private set; }
    public LedgerAccountType AccountType { get; private set; }
    public bool IsSystemAccount { get; private set; }

    public LedgerAccount? ParentAccount { get; private set; }
    public LedgerAccountId? ParentAccountId { get; private set; }

    public PriceUnit? PriceUnit { get; private set; }
    public PriceUnitId? PriceUnitId { get; private set; }

    public IReadOnlyList<FinancialAccount>? FinancialAccounts { get; private set; }

    public CustomerId? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public static LedgerAccount CreateSystemAccount(string title, LedgerAccountType accountType, LedgerAccountId? parentAccountId = null, PriceUnitId? priceUnitId = null)
    {
        return new LedgerAccount
        {
            Id = new LedgerAccountId(Guid.NewGuid()),
            Title = title,
            AccountType = accountType,
            ParentAccountId = parentAccountId,
            PriceUnitId = priceUnitId,
            IsSystemAccount = true
        };
    }

    public static LedgerAccount CreateCustomerAccount(string title,
        CustomerId customerId,
        PriceUnitId priceUnitId,
        LedgerAccountType accountType,
        LedgerAccountId? parentAccountId = null)
    {
        return new LedgerAccount
        {
            Id = new LedgerAccountId(Guid.NewGuid()),
            Title = title,
            CustomerId = customerId,
            AccountType = accountType,
            IsSystemAccount = false,
            ParentAccountId = parentAccountId,
            PriceUnitId = priceUnitId
        };
    }

    public void SetTitle(string title) => Title = title;
    public void SetParentAccount(LedgerAccountId? parentAccountId) => ParentAccountId = parentAccountId;
    public void SetPriceUnitId(PriceUnitId? priceUnitId) => PriceUnitId = priceUnitId;

#pragma warning disable CS8618
    private LedgerAccount() { }
#pragma warning restore CS8618
}