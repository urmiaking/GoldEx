using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Domain.Aggregates.InvoiceAggregate;

public class InvoiceDebtBase : EntityBase
{
    public InvoiceDebtBase(double amount, UnitType unitType, DateTime dueDate)
    {
        Amount = amount;
        UnitType = unitType;
        DueDate = dueDate;
    }

    protected InvoiceDebtBase() { }

    public double Amount { get; private set; }
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

    public void SetAmount(double amount, UnitType unitType)
    {
        Amount = amount;
        UnitType = unitType;
    }

    public void SetDueDate(DateTime dueDate) => DueDate = dueDate;
}