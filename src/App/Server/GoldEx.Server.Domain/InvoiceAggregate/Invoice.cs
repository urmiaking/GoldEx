using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceItemAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public readonly record struct InvoiceId(Guid Value);
public class Invoice : EntityBase<InvoiceId>
{
    public static Invoice Create(long invoiceNumber, CustomerId customerId, PriceUnitId priceUnitId, DateOnly invoiceDate, DateOnly? dueDate)
    {
        return new Invoice
        {
            Id = new InvoiceId(Guid.NewGuid()),
            InvoiceNumber = invoiceNumber,
            CustomerId = customerId,
            InvoiceDate = invoiceDate,
            PriceUnitId = priceUnitId,
            DueDate = dueDate
        };
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Invoice() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public long InvoiceNumber { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public DateOnly InvoiceDate { get; private set; }

    public void SetInvoiceNumber(long invoiceNumber) => InvoiceNumber = invoiceNumber;
    public void SetDueDate(DateOnly dueDate) => DueDate = dueDate;
    public void SetInvoiceDate(DateOnly invoiceDate) => InvoiceDate = invoiceDate;

    #region Customer

    public CustomerId CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public Invoice SetCustomerId(CustomerId customerId)
    {
        CustomerId = customerId;
        return this;
    }

    #endregion

    #region InvoiceItems

    public IReadOnlyList<InvoiceItem> Items { get; private set; }

    #endregion

    #region Payments

    private readonly List<InvoicePayment> _invoicePayments = [];
    public IReadOnlyList<InvoicePayment> InvoicePayments => _invoicePayments;

    public Invoice SetInvoicePayments(IEnumerable<InvoicePayment>? invoicePayments)
    {
        ClearInvoicePayments();

        if (invoicePayments is not null)
            _invoicePayments.AddRange(invoicePayments);

        return this;
    }

    public void ClearInvoicePayments() => _invoicePayments.Clear();

    #endregion

    #region ExtraCosts

    private readonly List<InvoiceExtraCost> _extraCosts = [];
    public IReadOnlyList<InvoiceExtraCost> ExtraCosts => _extraCosts;

    public Invoice SetExtraCosts(IEnumerable<InvoiceExtraCost>? extraCosts)
    {
        ClearExtraCosts();

        if (extraCosts is not null)
            _extraCosts.AddRange(extraCosts);

        return this;
    }

    public void ClearExtraCosts() => _extraCosts.Clear();

    #endregion

    #region Discounts

    private readonly List<InvoiceDiscount> _discounts = [];
    public IReadOnlyList<InvoiceDiscount> Discounts => _discounts;

    public Invoice SetDiscounts(IEnumerable<InvoiceDiscount>? discounts)
    {
        ClearDiscounts();

        if (discounts is not null)
            _discounts.AddRange(discounts);

        return this;
    }

    public void ClearDiscounts() => _discounts.Clear();

    #endregion

    #region Unit

    public PriceUnit? PriceUnit { get; private set; }
    public PriceUnitId PriceUnitId { get; private set; }

    #endregion

    #region Calculations

    public decimal TotalTaxAmount => Items.Sum(item => item.ItemTaxAmount);

    public decimal TotalAmount => Items.Sum(item => item.TotalAmount);

    public decimal TotalWageAmount => Items.Sum(item => item.ItemWageAmount);

    public decimal TotalProfitAmount => Items.Sum(item => item.ItemProfitAmount);

    public decimal TotalRawAmount => Items.Sum(item => item.ItemRawAmount);

    public decimal TotalPaidAmount => InvoicePayments.Sum(payment => payment.Amount * (payment.ExchangeRate ?? 1));

    public decimal TotalDiscountAmount => Discounts.Sum(discount => discount.Amount * (discount.ExchangeRate ?? 1));

    public decimal TotalExtraCostAmount => ExtraCosts.Sum(extraCost => extraCost.Amount * (extraCost.ExchangeRate ?? 1));

    public decimal TotalAmountWithDiscountsAndExtraCosts => TotalAmount - TotalDiscountAmount + TotalExtraCostAmount;

    public decimal TotalUnpaidAmount => TotalAmountWithDiscountsAndExtraCosts - TotalPaidAmount;

    #endregion
}