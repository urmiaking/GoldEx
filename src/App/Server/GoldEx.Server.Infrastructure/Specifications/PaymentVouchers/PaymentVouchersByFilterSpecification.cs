using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.PaymentVouchers;

public class PaymentVouchersByFilterSpecification : SpecificationBase<PaymentVoucher>
{
    public PaymentVouchersByFilterSpecification(RequestFilter filter, PaymentVoucherFilter voucherFilter, CustomerId? customerId)
    {
        // --- Basic Query Setup ---
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;
        ApplyPaging(skip, take);

        // --- Includes ---
        AddInclude(x => x.Customer!);
        AddInclude(x => x.FinancialAccount!);
        AddInclude(x => x.VoucherPriceUnit!);
        AddInclude(x => x.AmountPriceUnit!);

        // --- Main Filtering Logic ---

        // Apply customer filter if provided
        if (customerId.HasValue)
        {
            AddCriteria(x => x.CustomerId == customerId.Value);
        }

        // Apply date range filter on InvoiceDate
        if (voucherFilter.Start.HasValue)
        {
            var startDateOnly = DateOnly.FromDateTime(voucherFilter.Start.Value.Date);
            AddCriteria(x => x.PaymentDate >= startDateOnly);
        }
        if (voucherFilter.End.HasValue)
        {
            var endDateOnly = DateOnly.FromDateTime(voucherFilter.End.Value.Date);
            AddCriteria(x => x.PaymentDate <= endDateOnly);
        }

        // Apply status filter only if a status is provided
        if (voucherFilter.VoucherStatus.HasValue)
        {
            switch (voucherFilter.VoucherStatus.Value)
            {
                case VoucherStatus.Pending:
                    break;
                case VoucherStatus.Applied:
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
                    x.VoucherNumber == number);
            }
            else
            {
                AddCriteria(x =>
                    x.Customer!.FullName.Contains(filter.Search) ||
                    x.Description.Contains(filter.Search));
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