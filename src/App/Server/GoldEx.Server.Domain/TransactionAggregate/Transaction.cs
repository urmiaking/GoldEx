using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.InventoryEntryAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.MeltingBatchAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.TransactionAggregate;

public readonly record struct TransactionId(Guid Value);
public class Transaction : EntityBase<TransactionId>
{
    public static Transaction CreateForInvoice(
        string description,
        decimal amount,
        decimal? exchangeRate,
        Guid groupId,
        TransactionType transactionType,
        LedgerAccountId ledgerAccountId,
        PriceUnitId priceUnitId,
        InvoiceId invoiceId,
        DateTime postingDate)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty.", nameof(description));

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");

        if (exchangeRate is <= 0)
            throw new ArgumentOutOfRangeException(nameof(exchangeRate), "Exchange rate must be greater than zero if provided.");

        if (groupId == Guid.Empty)
            throw new ArgumentException("Group ID cannot be empty.", nameof(groupId));

        return new Transaction
        {
            Id = new TransactionId(Guid.NewGuid()),
            Description = description,
            Amount = amount,
            GroupId = groupId,
            TransactionType = transactionType,
            LedgerAccountId = ledgerAccountId,
            ExchangeRate = exchangeRate,
            PriceUnitId = priceUnitId,
            InvoiceId = invoiceId,
            BaseCurrencyAmount = amount * (exchangeRate ?? 1),
            PostingDate = postingDate
        };
    }

    public static Transaction CreateForPaymentVoucher(
        string description,
        decimal amount,
        decimal? exchangeRate,
        Guid groupId,
        TransactionType transactionType,
        LedgerAccountId ledgerAccountId,
        PriceUnitId priceUnitId,
        PaymentVoucherId paymentVoucherId,
        DateTime postingDate)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty.", nameof(description));

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");

        if (exchangeRate is <= 0)
            throw new ArgumentOutOfRangeException(nameof(exchangeRate), "Exchange rate must be greater than zero if provided.");

        if (groupId == Guid.Empty)
            throw new ArgumentException("Group ID cannot be empty.", nameof(groupId));

        return new Transaction
        {
            Id = new TransactionId(Guid.NewGuid()),
            Description = description,
            Amount = amount,
            GroupId = groupId,
            TransactionType = transactionType,
            LedgerAccountId = ledgerAccountId,
            ExchangeRate = exchangeRate,
            PriceUnitId = priceUnitId,
            PaymentVoucherId = paymentVoucherId,
            BaseCurrencyAmount = amount * (exchangeRate ?? 1),
            PostingDate = postingDate
        };
    }

    public static Transaction CreateForInvoicePayment(
        string description,
        decimal amount,
        decimal? exchangeRate,
        Guid groupId,
        TransactionType transactionType,
        LedgerAccountId ledgerAccountId,
        PriceUnitId priceUnitId,
        InvoiceId invoiceId,
        InvoicePaymentId invoicePaymentId,
        DateTime postingDate)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty.", nameof(description));

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");

        if (groupId == Guid.Empty)
            throw new ArgumentException("Group ID cannot be empty.", nameof(groupId));

        return new Transaction
        {
            Id = new TransactionId(Guid.NewGuid()),
            Description = description,
            Amount = amount,
            GroupId = groupId,
            TransactionType = transactionType,
            LedgerAccountId = ledgerAccountId,
            ExchangeRate = exchangeRate,
            PriceUnitId = priceUnitId,
            InvoiceId = invoiceId,
            InvoicePaymentId = invoicePaymentId,
            BaseCurrencyAmount = amount * (exchangeRate ?? 1),
            PostingDate = postingDate
        };
    }

    public static Transaction CreateForManualEntry(string description,
        decimal amount,
        decimal? exchangeRate,
        Guid groupId,
        TransactionType transactionType,
        LedgerAccountId ledgerAccountId,
        PriceUnitId priceUnitId, 
        InvoiceId invoiceId,
        DateTime postingDate)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty.", nameof(description));
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than or equal to zero.");
        if (exchangeRate is <= 0)
            throw new ArgumentOutOfRangeException(nameof(exchangeRate), "Exchange rate must be greater than zero if provided.");
        
        return new Transaction
        {
            Id = new TransactionId(Guid.NewGuid()),
            Description = description,
            Amount = amount,
            GroupId = groupId,
            TransactionType = transactionType,
            LedgerAccountId = ledgerAccountId,
            ExchangeRate = exchangeRate,
            PriceUnitId = priceUnitId,
            BaseCurrencyAmount = amount * (exchangeRate ?? 1),
            InvoiceId = invoiceId,
            PostingDate = postingDate
        };
    }

#pragma warning disable CS8618
    private Transaction() { }
#pragma warning restore CS8618

    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public decimal? ExchangeRate { get; private set; }
    public decimal BaseCurrencyAmount { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public Guid GroupId { get; private set; }
    public DateTime PostingDate { get; private set; }

    public PriceUnitId PriceUnitId { get; private set; }
    public PriceUnit? PriceUnit { get; private set; }

    public LedgerAccountId LedgerAccountId { get; private set; }
    public LedgerAccount? LedgerAccount { get; private set; }

    public InvoiceId? InvoiceId { get; private set; }
    public Invoice? Invoice { get; private set; }

    public PaymentVoucherId? PaymentVoucherId { get; private set; }
    public PaymentVoucher? PaymentVoucher { get; private set; }

    public InvoicePaymentId? InvoicePaymentId { get; private set; }
    public InvoicePayment? InvoicePayment { get; private set; }

    public MeltingBatchId? MeltingBatchId { get; private set; }
    public MeltingBatch? MeltingBatch { get; private set; }

    public TransactionId? ReverseTransactionId { get; private set; }
    public Transaction? ReverseTransaction { get; set; }

    public InventoryEntryId? InventoryEntryId { get; private set; }
    public InventoryEntry? InventoryEntry { get; private set; }

    public static Transaction CreateForMeltingBatch(string description,
        decimal amount,
        decimal? exchangeRate,
        decimal baseCurrencyAmount,
        TransactionType transactionType,
        Guid groupId,
        PriceUnitId priceUnitId,
        LedgerAccountId ledgerAccountId,
        InvoiceId invoiceId,
        MeltingBatchId meltingBatchId,
        DateTime postingDate)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty.", nameof(description));
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than or equal to zero.");
        if (exchangeRate is <= 0)
            throw new ArgumentOutOfRangeException(nameof(exchangeRate), "Exchange rate must be greater than zero if provided.");

        return new Transaction
        {
            Id = new TransactionId(Guid.NewGuid()),
            Description = description,
            Amount = amount,
            BaseCurrencyAmount = baseCurrencyAmount,
            TransactionType = transactionType,
            ExchangeRate = exchangeRate,
            GroupId = groupId,
            InvoiceId = invoiceId,
            PriceUnitId = priceUnitId,
            LedgerAccountId = ledgerAccountId,
            MeltingBatchId = meltingBatchId,
            PostingDate = postingDate
        };
    }

    public static Transaction CreateForMoltenGold(string description,
        decimal amount,
        decimal? exchangeRate,
        decimal baseCurrencyAmount,
        TransactionType transactionType,
        Guid groupId,
        PriceUnitId priceUnitId,
        LedgerAccountId ledgerAccountId,
        MeltingBatchId meltingBatchId,
        DateTime postingDate)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty.", nameof(description));

        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than or equal to zero.");
        
        return new Transaction
        {
            Id = new TransactionId(Guid.NewGuid()),
            Description = description,
            Amount = amount,
            BaseCurrencyAmount = baseCurrencyAmount,
            TransactionType = transactionType,
            ExchangeRate = exchangeRate,
            GroupId = groupId,
            PriceUnitId = priceUnitId,
            LedgerAccountId = ledgerAccountId,
            MeltingBatchId = meltingBatchId,
            PostingDate = postingDate
        };
    }

    public Transaction MarkAsReversalOf(TransactionId originalTransactionId)
    {
        ReverseTransactionId = originalTransactionId;
        return this;
    }

    public static Transaction CreateForInventoryEntry(string description,
        decimal amount,
        decimal baseCurrencyAmount,
        decimal? exchangeRate,
        Guid groupId,
        TransactionType transactionType,
        LedgerAccountId ledgerAccountId,
        PriceUnitId priceUnitId,
        InventoryEntryId inventoryEntryId,
        DateTime postingDate)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty.", nameof(description));

        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than or equal to zero.");

        if (baseCurrencyAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(baseCurrencyAmount), "Base currency amount must be greater than or equal to zero.");

        if (exchangeRate is <= 0)
            throw new ArgumentOutOfRangeException(nameof(exchangeRate),
                "Exchange rate must be greater than zero if provided.");

        return new Transaction
        {
            Id = new TransactionId(Guid.NewGuid()),
            Description = description,
            Amount = amount,
            BaseCurrencyAmount = baseCurrencyAmount,
            GroupId = groupId,
            TransactionType = transactionType,
            LedgerAccountId = ledgerAccountId,
            ExchangeRate = exchangeRate,
            PriceUnitId = priceUnitId,
            InventoryEntryId = inventoryEntryId,
            PostingDate = postingDate
        };
    }
}