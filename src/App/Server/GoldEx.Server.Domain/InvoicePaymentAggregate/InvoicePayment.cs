using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Domain.InvoicePaymentAggregate;

public readonly record struct InvoicePaymentId(Guid Value);
public class InvoicePayment : EntityBase<InvoicePaymentId>
{
    public static InvoicePayment Create(
        DateTime paymentDate,
        decimal amount,
        decimal? exchangeRate,
        InvoiceId invoiceId,
        PriceUnitId priceUnitId,
        FinancialAccountId? sourceFinancialAccountId,
        PaymentVoucherId? paymentVoucherId,
        string? referenceNumber = null,
        string? note = null)
    {
        return new InvoicePayment
        {
            Id = new InvoicePaymentId(Guid.NewGuid()),
            PaymentDate = paymentDate,
            Amount = amount,
            InvoiceId = invoiceId,
            PriceUnitId = priceUnitId,
            ExchangeRate = exchangeRate,
            SourceFinancialAccountId = sourceFinancialAccountId,
            PaymentVoucherId = paymentVoucherId,
            ReferenceNumber = referenceNumber,
            Note = note
        };
    }

    private InvoicePayment() { }

    public DateTime PaymentDate { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? Note { get; private set; }

    public decimal Amount { get; private set; }
    public decimal? ExchangeRate { get; private set; }

    public PriceUnitId PriceUnitId { get; private set; }
    public PriceUnit? PriceUnit { get; private set; }

    public FinancialAccountId? SourceFinancialAccountId { get; private set; }
    public FinancialAccount? SourceFinancialAccount { get; private set; }

    public PaymentVoucherId? PaymentVoucherId { get; private set; }
    public PaymentVoucher? PaymentVoucher { get; private set; }

    public InvoiceId InvoiceId { get; private set; }
    public Invoice? Invoice { get; private set; }

    public void SetPaymentDate(DateTime paymentDate) => PaymentDate = paymentDate;
    public void SetReferenceNumber(string? referenceNumber) => ReferenceNumber = referenceNumber;
    public void SetNote(string? note) => Note = note;
    public void SetAmount(decimal amount, PriceUnitId amountUnitId)
    {
        Amount = amount;
        PriceUnitId = amountUnitId;
    }
    public void SetPaymentVoucherId(PaymentVoucherId? paymentVoucherId) => PaymentVoucherId = paymentVoucherId;
    public void SetSourceFinancialAccountId(FinancialAccountId? financialAccountId) =>
        SourceFinancialAccountId = financialAccountId;
}