using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public class InvoiceDebt : EntityBase
{
    public InvoiceDebt(decimal amount, UnitType unitType, DateTime dueDate)
    {
        Amount = amount;
        UnitType = unitType;
        DueDate = dueDate;
    }

    private InvoiceDebt() { }

    public decimal Amount { get; private set; }
    public UnitType UnitType { get; private set; }
    public DateTime DueDate { get; private set; }
    public bool IsPaid { get; set; }
    public DateTime? PaymentDate { get; set; }

    public void SetPaid(DateTime paymentDate)
    {
        IsPaid = true;
        PaymentDate = paymentDate;
    }

    public void SetUnpaid()
    {
        IsPaid = false;
        PaymentDate = null;
    }

    public void SetAmount(decimal amount, UnitType unitType)
    {
        Amount = amount;
        UnitType = unitType;
    }

    public void SetDueDate(DateTime dueDate) => DueDate = dueDate;
}