using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.PaymentMethodAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public class InvoicePayment : EntityBase
{
    public static InvoicePayment Create(
        DateTime paymentDate,
        decimal amount,
        decimal? exchangeRate,
        PriceUnitId priceUnitId,
        PaymentMethodId paymentMethodId,
        string? referenceNumber = null,
        string? note = null)
    {
        return new InvoicePayment
        {
            PaymentDate = paymentDate,
            Amount = amount,
            PriceUnitId = priceUnitId,
            ExchangeRate = exchangeRate,
            PaymentMethodId = paymentMethodId,
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

    public PaymentMethodId PaymentMethodId { get; private set; }
    public PaymentMethod? PaymentMethod { get; private set; }

    public void SetPaymentDate(DateTime paymentDate) => PaymentDate = paymentDate;
    public void SetReferenceNumber(string? referenceNumber) => ReferenceNumber = referenceNumber;
    public void SetNote(string? note) => Note = note;
    public void SetAmount(decimal amount, PriceUnitId amountUnitId)
    {
        Amount = amount;
        PriceUnitId = amountUnitId;
    }
    public void SetPaymentMethodId(PaymentMethodId paymentMethodId) => PaymentMethodId = paymentMethodId;
}