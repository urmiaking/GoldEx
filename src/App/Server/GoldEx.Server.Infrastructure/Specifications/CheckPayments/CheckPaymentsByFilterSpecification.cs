using System.Linq;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CheckPaymentAggregate;
using GoldEx.Shared.DTOs.CheckPayments;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.CheckPayments;

public class CheckPaymentsByFilterSpecification : SpecificationBase<CheckPayment>
{
    public CheckPaymentsByFilterSpecification(RequestFilter filter, CheckPaymentFilter checkPaymentFilter)
    {
        // Paging
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;
        ApplyPaging(skip, take);

        // Includes
        AddInclude(x => x.Issuer!);
        AddInclude(x => x.IssuerFinancialAccount!);
        AddInclude(x => x.InvoicePayment!);
        AddInclude(x => x.InvoicePayment!.PriceUnit!);
        AddInclude(x => x.InvoicePayment!.Invoice!);
        AddInclude(x => x.ChangeLogs);

        // Filter by Status
        if (checkPaymentFilter.Status.HasValue)
        {
            AddCriteria(x => x.ChangeLogs
                .OrderByDescending(cl => cl.DateTime)
                .Select(cl => (CheckPaymentStatus?)cl.Status)
                .FirstOrDefault() == checkPaymentFilter.Status.Value);
        }

        // Filter by Due Date Range
        if (checkPaymentFilter.StartDueDate.HasValue)
        {
            AddCriteria(x => x.DueDate >= checkPaymentFilter.StartDueDate.Value);
        }
        if (checkPaymentFilter.EndDueDate.HasValue)
        {
            AddCriteria(x => x.DueDate <= checkPaymentFilter.EndDueDate.Value);
        }

        // Search text filter
        if (!string.IsNullOrEmpty(filter.Search))
        {
            AddCriteria(x =>
                (x.Number != null && x.Number.Contains(filter.Search)) ||
                (x.SayadiCode != null && x.SayadiCode.Contains(filter.Search)) ||
                (x.Issuer != null && x.Issuer.FullName.Contains(filter.Search)));
        }

        // Sorting
        var sortLabel = filter.SortLabel;
        if (string.Equals(sortLabel, "IssuerFullName", StringComparison.OrdinalIgnoreCase))
            sortLabel = "Issuer.FullName";
        else if (string.Equals(sortLabel, "IssuerFinancialAccountName", StringComparison.OrdinalIgnoreCase))
            sortLabel = "IssuerFinancialAccount.Name";
        else if (string.Equals(sortLabel, "Amount", StringComparison.OrdinalIgnoreCase))
            sortLabel = "InvoicePayment.Amount";
        else if (string.Equals(sortLabel, "CurrentStatus", StringComparison.OrdinalIgnoreCase))
            sortLabel = null; // Fallback to default sorting because status is a subquery.

        if (!string.IsNullOrEmpty(sortLabel) && filter.SortDirection.HasValue && filter.SortDirection != SortDirection.None)
        {
            ApplySorting(sortLabel, filter.SortDirection.Value);
        }
        else
        {
            ApplySorting(nameof(CheckPayment.DueDate), SortDirection.Ascending);
        }
    }
}
