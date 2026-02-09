using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.InvoicePaymentAggregate;

public readonly record struct InvoicePaymentId(Guid Value);
public class InvoicePayment : EntityBase<InvoicePaymentId>
{
    public static InvoicePayment Create(
        DateTime paymentDate,
        PaymentType paymentType,
        PaymentSide paymentSide,
        decimal amount,
        decimal? exchangeRate,
        decimal? goldFineness,
        InvoiceId invoiceId,
        PriceUnitId priceUnitId,
        FinancialAccountId? sourceFinancialAccountId,
        LedgerAccountId? ledgerAccountId,
        PaymentVoucherId? paymentVoucherId,
        InvoicePaymentId? sourcePaymentId,
        InvoiceId? targetInvoiceId,
        string? referenceNumber = null,
        string? note = null)
    {
        var finalAmount = goldFineness.HasValue ? amount * goldFineness.Value / 750m : amount;

        return new InvoicePayment
        {
            Id = new InvoicePaymentId(Guid.CreateVersion7()),
            PaymentType = paymentType,
            PaymentSide = paymentSide,
            PaymentDate = paymentDate,
            GoldFineness = goldFineness,
            Amount = amount,
            FinalAmount = finalAmount,
            InvoiceId = invoiceId,
            PriceUnitId = priceUnitId,
            ExchangeRate = exchangeRate,
            SourceFinancialAccountId = sourceFinancialAccountId,
            TargetInvoiceId = targetInvoiceId,
            LedgerAccountId = ledgerAccountId,
            PaymentVoucherId = paymentVoucherId,
            SourcePaymentId = sourcePaymentId,
            ReferenceNumber = referenceNumber,
            Note = note
        };
    }

    private InvoicePayment() { }

    public PaymentType PaymentType { get; private set; }
    public PaymentSide PaymentSide { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? Note { get; private set; }

    public decimal Amount { get; private set; }
    public decimal? ExchangeRate { get; private set; }
    public decimal? GoldFineness { get; private set; }

    public PriceUnitId PriceUnitId { get; private set; }
    public PriceUnit? PriceUnit { get; private set; }

    public FinancialAccountId? SourceFinancialAccountId { get; private set; }
    public FinancialAccount? SourceFinancialAccount { get; private set; }

    public LedgerAccountId? LedgerAccountId { get; private set; }
    public LedgerAccount? LedgerAccount { get; private set; }

    public PaymentVoucherId? PaymentVoucherId { get; private set; }
    public PaymentVoucher? PaymentVoucher { get; private set; }

    public InvoiceId InvoiceId { get; private set; }
    public Invoice? Invoice { get; private set; }

    public InvoicePaymentId? SourcePaymentId { get; private set; }
    public InvoicePayment? SourcePayment { get; private set; }

    public InvoiceId? TargetInvoiceId { get; private set; }
    public Invoice? TargetInvoice { get; private set; }

    public decimal FinalAmount { get; private set; }

    public void SetPaymentDate(DateTime paymentDate) => PaymentDate = paymentDate;
    public void SetReferenceNumber(string? referenceNumber) => ReferenceNumber = referenceNumber;
    public void SetNote(string? note) => Note = note;
    public void SetAmount(decimal amount, PriceUnitId amountUnitId)
    {
        Amount = amount;
        PriceUnitId = amountUnitId;
    }
    public void SetExchangeRate(decimal? exchangeRate) => ExchangeRate = exchangeRate;
    public void SetPaymentVoucherId(PaymentVoucherId? paymentVoucherId) => PaymentVoucherId = paymentVoucherId;
    public void SetSourceFinancialAccountId(FinancialAccountId? financialAccountId) =>
        SourceFinancialAccountId = financialAccountId;
    public void SetLedgerAccountId(LedgerAccountId? ledgerAccountId) => LedgerAccountId = ledgerAccountId;
    public void SetPaymentType(PaymentType paymentType) => PaymentType = paymentType;
    public void SetPaymentSide(PaymentSide paymentSide) => PaymentSide = paymentSide;

    public void SetFinalAmount(decimal amount, decimal? goldFineness)
    {
        GoldFineness = goldFineness;
        FinalAmount = goldFineness.HasValue ? amount * goldFineness.Value / 750m : amount;
    }

    /// <summary>
    /// This method is used to set the LedgerAccount navigation property.
    /// </summary>
    /// <param name="ledgerAccount"></param>
    public void SetLedgerAccount(LedgerAccount? ledgerAccount)
    {
        LedgerAccount = ledgerAccount;
    }

    /// <summary>
    /// This method is used to set the Invoice navigation property.
    /// </summary>
    /// <param name="invoice"></param>
    public void SetInvoice(Invoice invoice)
    {
        Invoice = invoice;
    }

    public void SetInvoiceId(InvoiceId targetInvoiceId)
    {
        InvoiceId = targetInvoiceId;
    }

    public void SetTargetInvoiceId(InvoiceId? targetInvoiceId)
    {
        TargetInvoiceId = targetInvoiceId;
    }
}