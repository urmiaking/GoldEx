using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.CustomerTransferVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Shared.DTOs.CustomerTransfers;

namespace GoldEx.Server.Infrastructure.Specifications.CustomerTransfers;

public class CustomerTransferVouchersByFilterSpecification : SpecificationBase<CustomerTransferVoucher>
{
    public CustomerTransferVouchersByFilterSpecification(RequestFilter filter, CustomerTransferVoucherFilter voucherFilter)
    {
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;
        ApplyPaging(skip, take);

        AddInclude(x => x.SourceCustomer!);
        AddInclude(x => x.DestinationCustomer!);
        AddInclude(x => x.PriceUnit!);
        AddInclude(x => x.SourceInvoice!);
        AddInclude(x => x.DestinationInvoice!);

        if (voucherFilter.SourceCustomerId.HasValue)
        {
            AddCriteria(x => x.SourceCustomerId == new CustomerId(voucherFilter.SourceCustomerId.Value));
        }

        if (voucherFilter.DestinationCustomerId.HasValue)
        {
            AddCriteria(x => x.DestinationCustomerId == new CustomerId(voucherFilter.DestinationCustomerId.Value));
        }

        if (voucherFilter.PriceUnitId.HasValue)
        {
            AddCriteria(x => x.PriceUnitId == new PriceUnitId(voucherFilter.PriceUnitId.Value));
        }

        if (voucherFilter.FromDate.HasValue)
        {
            AddCriteria(x => x.TransferDate >= voucherFilter.FromDate.Value);
        }

        if (voucherFilter.ToDate.HasValue)
        {
            AddCriteria(x => x.TransferDate <= voucherFilter.ToDate.Value);
        }

        var search = filter.Search ?? voucherFilter.SearchTerm;
        if (!string.IsNullOrWhiteSpace(search))
        {
            if (long.TryParse(search, out var number))
            {
                AddCriteria(x => x.VoucherNumber == number);
            }
            else if (Guid.TryParse(search, out var id))
            {
                AddCriteria(x => x.Id == new CustomerTransferVoucherId(id));
            }
            else
            {
                AddCriteria(x =>
                    x.SourceCustomer!.FullName.Contains(search) ||
                    x.DestinationCustomer!.FullName.Contains(search) ||
                    x.Description.Contains(search));
            }
        }

        if (!string.IsNullOrEmpty(filter.SortLabel) && filter.SortDirection.HasValue && filter.SortDirection != SortDirection.None)
        {
            ApplySorting(filter.SortLabel, filter.SortDirection.Value);
        }
        else
        {
            ApplySorting(nameof(CustomerTransferVoucher.CreatedAt), SortDirection.Descending);
        }
    }
}
