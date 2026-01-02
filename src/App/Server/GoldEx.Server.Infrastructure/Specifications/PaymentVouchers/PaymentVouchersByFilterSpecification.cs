using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Shared.DTOs.PaymentVouchers;

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
        AddInclude(x => x.SourceFinancialAccount!);
        AddInclude(x => x.DestinationFinancialAccount!);
        AddInclude(x => x.Customer!);

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

        // Apply status filter only if a VoucherType is provided
        if (voucherFilter.VoucherType.HasValue)
        {
            AddCriteria(x => x.VoucherType == voucherFilter.VoucherType.Value);
        }

        // --- Search Filter ---
        if (!string.IsNullOrEmpty(filter.Search))
        {
            if (long.TryParse(filter.Search, out var number))
            {
                AddCriteria(x =>
                    x.VoucherNumber == number);
            }
            else if (Guid.TryParse(filter.Search, out var id))
            {
                AddCriteria(x => x.Id == new PaymentVoucherId(id));
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
            ApplySorting(nameof(PaymentVoucher.CreatedAt), SortDirection.Descending);
        }
    }
}