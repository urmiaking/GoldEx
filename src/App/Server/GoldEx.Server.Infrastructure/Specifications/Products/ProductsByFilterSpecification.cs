using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.Products;

public class ProductsByFilterSpecification : SpecificationBase<Product>
{
    public ProductsByFilterSpecification(RequestFilter filter, ProductStatus status = ProductStatus.Available)
    {
        if (filter.Skip < 0)
            filter.Skip = 0;

        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        AddInclude(x => x.WagePriceUnit!);

        // Apply search filter
        if (!string.IsNullOrEmpty(filter.Search))
        {
            AddCriteria(x => x.Name.Contains(filter.Search) || x.Barcode.Contains(filter.Search));
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(filter.SortLabel) && filter.SortDirection != null && filter.SortDirection != SortDirection.None)
        {
            ApplySorting(filter.SortLabel, filter.SortDirection.Value);
        }
        else
        {
            ApplySorting(nameof(Customer.CreatedAt), SortDirection.Descending);
        }

        AddCriteria(x => x.ProductStatus == status);

        // Apply paging
        ApplyPaging(skip, take);
    }
}