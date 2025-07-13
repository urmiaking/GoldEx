using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.Invoices;

public class InvoicesByFilterSpecification : SpecificationBase<Invoice>
{
    public InvoicesByFilterSpecification(RequestFilter filter, InvoiceFilter invoiceFilter, CustomerId? customerId)
    {
        // --- Basic Query Setup ---
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;
        ApplyPaging(skip, take);

        // --- Includes ---
        AddInclude(x => x.Customer!);
        AddInclude(x => x.PriceUnit!);

        // --- Main Filtering Logic ---

        // Apply customer filter if provided
        if (customerId.HasValue)
        {
            AddCriteria(x => x.CustomerId == customerId.Value);
        }

        // Apply date range filter on InvoiceDate
        if (invoiceFilter.Start.HasValue)
        {
            var startDateOnly = DateOnly.FromDateTime(invoiceFilter.Start.Value.Date);
            AddCriteria(x => x.InvoiceDate >= startDateOnly);
        }
        if (invoiceFilter.End.HasValue)
        {
            var endDateOnly = DateOnly.FromDateTime(invoiceFilter.End.Value.Date);
            AddCriteria(x => x.InvoiceDate <= endDateOnly);
        }

        // Apply status filter only if a status is provided
        if (invoiceFilter.Status.HasValue)
        {
            // This is the full, translatable calculation for the total unpaid amount.
            System.Linq.Expressions.Expression<Func<Invoice, bool>> isPaidExpression = x =>
                Math.Abs(
                    (x.Items.Sum(i => i.TotalAmount) - x.Discounts.Sum(d => d.Amount * (d.ExchangeRate ?? 1)) + x.ExtraCosts.Sum(e => e.Amount * (e.ExchangeRate ?? 1))) - x.InvoicePayments.Sum(p => p.Amount * (p.ExchangeRate ?? 1))
                ) < 0.01m;

            // The opposite expression, for checking if there is debt.
            System.Linq.Expressions.Expression<Func<Invoice, bool>> hasDebtExpression = x =>
               Math.Abs(
                   (x.Items.Sum(i => i.TotalAmount) - x.Discounts.Sum(d => d.Amount * (d.ExchangeRate ?? 1)) + x.ExtraCosts.Sum(e => e.Amount * (e.ExchangeRate ?? 1))) - x.InvoicePayments.Sum(p => p.Amount * (p.ExchangeRate ?? 1))
               ) >= 0.01m;


            switch (invoiceFilter.Status.Value)
            {
                case InvoicePaymentStatus.Paid:
                    AddCriteria(isPaidExpression);
                    break;

                case InvoicePaymentStatus.Overdue:
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    // Combine the "has debt" logic with the due date check
                    AddCriteria(hasDebtExpression.And(x => x.DueDate.HasValue && x.DueDate.Value < today));
                    break;

                case InvoicePaymentStatus.HasDebt:
                    var todayForDebt = DateOnly.FromDateTime(DateTime.Today);
                    // Combine the "has debt" logic with the "not overdue" check
                    AddCriteria(hasDebtExpression.And(x => !x.DueDate.HasValue || x.DueDate.Value >= todayForDebt));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // --- Search Filter ---
        if (!string.IsNullOrEmpty(filter.Search))
        {
            if (long.TryParse(filter.Search, out var number))
            {
                AddCriteria(x =>
                    x.InvoiceNumber == number || x.Items.Any(i => i.Product!.Barcode == number.ToString()));
            }
            else
            {
                AddCriteria(x =>
                    x.Customer!.FullName.Contains(filter.Search) ||
                    x.Items.Any(i => i.Product!.Name.Contains(filter.Search)));
            }
        }

        // --- Sorting ---
        if (!string.IsNullOrEmpty(filter.SortLabel) && filter.SortDirection.HasValue && filter.SortDirection != SortDirection.None)
        {
            ApplySorting(filter.SortLabel, filter.SortDirection.Value);
        }
        else
        {
            ApplySorting(nameof(Invoice.InvoiceDate), SortDirection.Descending);
        }
    }
}

// You will need a simple helper class for the .And() method used above.
// Place this in a utility file in your project.
public static class PredicateBuilder
{
    public static System.Linq.Expressions.Expression<Func<T, bool>> And<T>(this System.Linq.Expressions.Expression<Func<T, bool>> left, System.Linq.Expressions.Expression<Func<T, bool>> right)
    {
        var invokedExpr = System.Linq.Expressions.Expression.Invoke(right, left.Parameters);
        return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(System.Linq.Expressions.Expression.AndAlso(left.Body, invokedExpr), left.Parameters);
    }
}