using System.ComponentModel.DataAnnotations.Schema;
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
        decimal amount,
        decimal? exchangeRate,
        decimal? goldFineness,
        InvoiceId invoiceId,
        PriceUnitId priceUnitId,
        FinancialAccountId? sourceFinancialAccountId,
        LedgerAccountId? ledgerAccountId,
        PaymentVoucherId? paymentVoucherId,
        string? referenceNumber = null,
        string? note = null)
    {
        var finalAmount = goldFineness.HasValue ? amount * goldFineness.Value / 750m : amount;

        return new InvoicePayment
        {
            Id = new InvoicePaymentId(Guid.NewGuid()),
            PaymentType = paymentType,
            PaymentDate = paymentDate,
            GoldFineness = goldFineness,
            Amount = amount,
            FinalAmount = finalAmount,
            InvoiceId = invoiceId,
            PriceUnitId = priceUnitId,
            ExchangeRate = exchangeRate,
            SourceFinancialAccountId = sourceFinancialAccountId,
            LedgerAccountId = ledgerAccountId,
            PaymentVoucherId = paymentVoucherId,
            ReferenceNumber = referenceNumber,
            Note = note
        };
    }

    private InvoicePayment() { }

    public PaymentType PaymentType { get; private set; }
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
}