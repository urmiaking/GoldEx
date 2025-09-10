using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Shared.Enums;
using System.Diagnostics.CodeAnalysis;

namespace GoldEx.Server.Domain.FinancialAccountAggregate;

public readonly record struct FinancialAccountId(Guid Value);

public class FinancialAccount : EntityBase<FinancialAccountId>
{
    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    [ExcludeFromCodeCoverage] // Exclude from test coverage as it's for ORM use only.
#pragma warning disable CS8618
    private FinancialAccount() { }
#pragma warning restore CS8618

    /// <summary>
    /// The single, private constructor to create a valid FinancialAccount instance.
    /// </summary>
    private FinancialAccount(
        FinancialAccountType accountType,
        PriceUnitId priceUnitId,
        bool isSystemAccount,
        string? holderName,
        string? brokerName,
        CustomerId? customerId,
        LedgerAccountId? ledgerAccountId,
        LocalBankAccount? localAccount,
        InternationalBankAccount? internationalAccount,
        CashAccount? cashAccount)
    {
        Id = new FinancialAccountId(Guid.NewGuid());
        AccountType = accountType;
        PriceUnitId = priceUnitId;
        IsSystemAccount = isSystemAccount;
        HolderName = holderName;
        BrokerName = brokerName;
        CustomerId = customerId;
        LedgerAccountId = ledgerAccountId;
        LocalAccount = localAccount;
        InternationalAccount = internationalAccount;
        CashAccount = cashAccount;
    }

    // --- Core Properties ---
    public FinancialAccountType AccountType { get; private set; }
    public string? HolderName { get; private set; }
    public string? BrokerName { get; private set; }
    public bool IsSystemAccount { get; private set; }

    // --- Identifiers & Navigation Properties ---
    public PriceUnitId PriceUnitId { get; private set; }
    public PriceUnit? PriceUnit { get; private set; }

    public CustomerId? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public LedgerAccountId? LedgerAccountId { get; private set; }
    public LedgerAccount? LedgerAccount { get; private set; }

    // --- Account-Specific Details (Value Objects) ---
    public LocalBankAccount? LocalAccount { get; private set; }
    public InternationalBankAccount? InternationalAccount { get; private set; }
    public CashAccount? CashAccount { get; private set; }

    /// <summary>
    /// Creates a financial account linked to a customer.
    /// </summary>
    public static FinancialAccount CreateCustomerAccount(
        string? holderName,
        string? brokerName,
        FinancialAccountType accountType,
        PriceUnitId priceUnitId,
        CustomerId customerId,
        LocalBankAccount? localAccount = null,
        InternationalBankAccount? internationalAccount = null,
        CashAccount? cashAccount = null)
    {
        ValidateAccountDetails(accountType, holderName, brokerName, localAccount, internationalAccount, cashAccount);

        return new FinancialAccount(
            accountType, priceUnitId, false, holderName, brokerName,
            customerId, null, localAccount, internationalAccount, cashAccount);
    }

    /// <summary>
    /// Creates a system-owned financial account linked to a ledger.
    /// </summary>
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
        ValidateAccountDetails(accountType, holderName, brokerName, localAccount, internationalAccount, cashAccount);

        return new FinancialAccount(
            accountType, priceUnitId, true, holderName, brokerName,
            null, ledgerAccountId, localAccount, internationalAccount, cashAccount);
    }

    /// <summary>
    /// Updates the properties of an existing financial account.
    /// </summary>
    public void Update(
        string? holderName,
        string? brokerName,
        FinancialAccountType accountType,
        PriceUnitId priceUnitId,
        LocalBankAccount? localAccount,
        InternationalBankAccount? internationalAccount,
        CashAccount? cashAccount)
    {
        ValidateAccountDetails(accountType, holderName, brokerName, localAccount, internationalAccount, cashAccount);

        AccountType = accountType;
        PriceUnitId = priceUnitId;
        HolderName = holderName;
        BrokerName = brokerName;

        // Ensure only the correct account details object is populated
        LocalAccount = accountType == FinancialAccountType.LocalBankAccount ? localAccount : null;
        InternationalAccount = accountType == FinancialAccountType.InternationalBankAccount ? internationalAccount : null;
        CashAccount = accountType == FinancialAccountType.Cash ? cashAccount : null;
    }

    /// <summary>
    /// Centralized validation logic for creating or updating an account.
    /// </summary>
    private static void ValidateAccountDetails(
        FinancialAccountType accountType,
        string? holderName,
        string? brokerName,
        LocalBankAccount? localAccount,
        InternationalBankAccount? internationalAccount,
        CashAccount? cashAccount)
    {
        switch (accountType)
        {
            case FinancialAccountType.Cash:
                ArgumentNullException.ThrowIfNull(cashAccount, nameof(cashAccount));
                break;
            case FinancialAccountType.LocalBankAccount:
                ArgumentNullException.ThrowIfNull(localAccount, nameof(localAccount));
                break;
            case FinancialAccountType.InternationalBankAccount:
                ArgumentNullException.ThrowIfNull(internationalAccount, nameof(internationalAccount));
                break;
        }

        // Holder and Broker are required for non-internal cash accounts, or any non-gold account
        if (accountType != FinancialAccountType.Gold)
        {
            var isDepositsWithOthers = accountType == FinancialAccountType.Cash && cashAccount?.AccountType == CashAccountType.DepositsWithOthers;

            if (isDepositsWithOthers || accountType != FinancialAccountType.Cash)
            {
                ArgumentException.ThrowIfNullOrEmpty(holderName, nameof(holderName));
                ArgumentException.ThrowIfNullOrEmpty(brokerName, nameof(brokerName));
            }
        }
    }
}
