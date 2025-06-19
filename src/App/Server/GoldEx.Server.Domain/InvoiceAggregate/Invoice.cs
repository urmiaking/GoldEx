using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceItemAggregate;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public readonly record struct InvoiceId(Guid Value);
public class Invoice : EntityBase<InvoiceId>
{
    public static Invoice Create(long invoiceNumber, CustomerId customerId, DateOnly invoiceDate, DateOnly? dueDate)
    {
        return new Invoice
        {
            Id = new InvoiceId(Guid.NewGuid()),
            InvoiceNumber = invoiceNumber,
            CustomerId = customerId,
            InvoiceDate = invoiceDate,
            DueDate = dueDate
        };
    }

    private Invoice() { }

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

    public IReadOnlyList<InvoiceItem>? Items { get; private set; }

    #endregion

    #region Payments

    private readonly List<InvoicePayment> _invoicePayments = [];
    public IReadOnlyList<InvoicePayment> InvoicePayment => _invoicePayments;

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

    #region Calculations

    public decimal TotalTaxAmount => Items.Sum(item => item.ItemTaxAmount);

    public decimal TotalAmount => Items.Sum(item => item.TotalAmount);

    public decimal TotalWageAmount => Items.Sum(item => item.ItemWageAmount);

    public decimal TotalProfitAmount => Items.Sum(item => item.ItemProfitAmount);

    public decimal TotalRawAmount => Items.Sum(item => item.ItemRawAmount);

    public decimal TotalPaidAmount => InvoicePayment.Sum(payment => payment.Amount);

    public decimal TotalDiscountAmount => Discounts.Sum(discount => discount.Amount);

    public decimal TotalExtraCostAmount => ExtraCosts.Sum(extraCost => extraCost.Amount);

    public decimal TotalAmountWithDiscountsAndExtraCosts => TotalAmount - TotalDiscountAmount + TotalExtraCostAmount;

    public decimal TotalUnpaidAmount => TotalAmountWithDiscountsAndExtraCosts - TotalPaidAmount;

    #endregion
}