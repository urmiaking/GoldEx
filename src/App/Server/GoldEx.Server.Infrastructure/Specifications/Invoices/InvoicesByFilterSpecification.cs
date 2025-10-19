using System.Linq.Expressions;
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

        // Apply invoice type if provided
        if (invoiceFilter.InvoiceType.HasValue)
        {
            AddCriteria(x => x.InvoiceType == invoiceFilter.InvoiceType);
        }

        // Apply trade scale if provided
        if (invoiceFilter.TradeScale.HasValue)
        {
            AddCriteria(x => x.TradeScale == invoiceFilter.TradeScale);
        }

        // Apply status filter only if a status is provided
        if (invoiceFilter.Status.HasValue)
        {
            Expression<Func<Invoice, decimal>> unpaidAmountExpression = x =>
                (
                    x.ProductItems.Sum(i => i.ItemFinalAmount * (i.CostPriceExchangeRate ?? 1)) +
                    x.CoinItems.Sum(c => c.ItemFinalAmount) +
                    x.CurrencyItems.Sum(c => c.ItemFinalAmount) +
                    x.UsedProducts.Sum(up => up.ItemFinalAmount) -
                    x.Discounts.Sum(d => d.Amount * (d.ExchangeRate ?? 1)) +
                    x.ExtraCosts.Sum(e => e.Amount * (e.ExchangeRate ?? 1))
                )
                - x.InvoicePayments!.Sum(p => p.Amount * (p.ExchangeRate ?? 1))
                - (x.InvoiceType == InvoiceType.Sell ? x.UsedProducts.Sum(up => up.ItemFinalAmount) : 0);

            Expression<Func<Invoice, bool>> isPaidExpression = Expression.Lambda<Func<Invoice, bool>>(
                Expression.LessThan(
                    Expression.Call(typeof(Math), "Abs", null, unpaidAmountExpression.Body),
                    Expression.Constant(0.01m)
                ),
                unpaidAmountExpression.Parameters
            );

            Expression<Func<Invoice, bool>> hasDebtExpression = Expression.Lambda<Func<Invoice, bool>>(
                Expression.GreaterThanOrEqual(
                    Expression.Call(typeof(Math), "Abs", null, unpaidAmountExpression.Body),
                    Expression.Constant(0.01m)
                ),
                unpaidAmountExpression.Parameters
            );

            switch (invoiceFilter.Status.Value)
            {
                case InvoicePaymentStatus.Paid:
                    AddCriteria(isPaidExpression);
                    break;

                case InvoicePaymentStatus.Overdue:
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    AddCriteria(hasDebtExpression.And(x => x.DueDate.HasValue && x.DueDate.Value < today));
                    break;

                case InvoicePaymentStatus.HasDebt:
                    var todayForDebt = DateOnly.FromDateTime(DateTime.Today);
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
                    x.InvoiceNumber == number || x.ProductItems.Any(i => i.Product!.Barcode == number.ToString()));
            }
            else
            {
                AddCriteria(x =>
                    x.Customer!.FullName.Contains(filter.Search) ||
                    x.ProductItems.Any(i => i.Product!.Name.Contains(filter.Search)));
            }
        }

        // --- Sorting ---
        if (!string.IsNullOrEmpty(filter.SortLabel) && filter.SortDirection.HasValue && filter.SortDirection != SortDirection.None)
        {
            ApplySorting(filter.SortLabel, filter.SortDirection.Value);
        }
        else
        {
            ApplySorting(nameof(Invoice.CreatedAt), SortDirection.Descending);
        }
    }
}

public static class PredicateBuilder
{
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var invokedExpr = Expression.Invoke(right, left.Parameters);
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left.Body, invokedExpr), left.Parameters);
    }
}