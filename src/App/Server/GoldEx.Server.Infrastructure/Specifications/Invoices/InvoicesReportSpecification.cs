using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Shared.Enums;
using System.Linq.Expressions;
using GoldEx.Server.Domain.CustomerAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Invoices;

public class InvoicesReportSpecification : SpecificationBase<Invoice>
{
    public InvoicesReportSpecification(InvoiceType invoiceType,
        InvoicePaymentStatus? paymentStatus = null,
        Guid? priceUnitId = null,
        Guid? customerId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        AddInclude(x => x.Customer!);
        AddInclude(x => x.InvoicePayments!);

        AddCriteria(x => x.InvoiceType == invoiceType);

        if (paymentStatus.HasValue)
        {
            var unpaidExpr = InvoiceExpressions.TotalUnpaidAmount();

            var isPaidExpression =
                Expression.Lambda<Func<Invoice, bool>>(
                    Expression.LessThan(
                        Expression.Call(
                            typeof(Math),
                            nameof(Math.Abs),
                            null,
                            unpaidExpr.Body
                        ),
                        Expression.Constant(0.01m)
                    ),
                    unpaidExpr.Parameters
                );

            var hasDebtExpression =
                Expression.Lambda<Func<Invoice, bool>>(
                    Expression.GreaterThanOrEqual(
                        Expression.Call(
                            typeof(Math),
                            nameof(Math.Abs),
                            null,
                            unpaidExpr.Body
                        ),
                        Expression.Constant(0.01m)
                    ),
                    unpaidExpr.Parameters
                );

            switch (paymentStatus.Value)
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

        if (priceUnitId.HasValue)
        {
            AddCriteria(x => x.PriceUnitId == new PriceUnitId(priceUnitId.Value));
        }

        if (customerId.HasValue)
        {
            AddCriteria(x => x.CustomerId == new CustomerId(customerId.Value));
        }

        if (fromDate.HasValue)
        {
            var from = DateOnly.FromDateTime(fromDate.Value);
            AddCriteria(x => x.InvoiceDate >= from);
        }

        if (toDate.HasValue)
        {
            var to = DateOnly.FromDateTime(toDate.Value);
            AddCriteria(x => x.InvoiceDate <= to);
        }

        ApplyOrderBy(x => x.InvoiceDate);
    }
}