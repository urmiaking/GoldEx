using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public abstract class InvoiceItemBase : EntityBase
{
    public decimal TotalPrice { get; protected set; }
    public decimal ItemTaxAmount { get; protected set; }
    public decimal TotalAmount { get; protected set; }
    public decimal ItemWageAmount { get; protected set; }
    public decimal ItemProfitAmount { get; protected set; }
    public decimal ItemRawAmount { get; protected set; }
}