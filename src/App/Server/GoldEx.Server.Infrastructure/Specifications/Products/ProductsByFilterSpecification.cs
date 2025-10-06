using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.DTOs.Products;

namespace GoldEx.Server.Infrastructure.Specifications.Products;

public class ProductsByFilterSpecification : SpecificationBase<Product>
{
    public ProductsByFilterSpecification(RequestFilter filter, ProductFilter productFilter)
    {
        ApplyPaging(Math.Max(0, filter.Skip ?? 0), filter.Take ?? 100);

        AddInclude(x => x.WagePriceUnit!);
        AddInclude(x => x.ProductCategory!);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            AddCriteria(x => x.Name.Contains(filter.Search) || x.Barcode.Contains(filter.Search));
        }

        if (!string.IsNullOrWhiteSpace(filter.SortLabel) && filter.SortDirection.HasValue && filter.SortDirection != SortDirection.None)
        {
            ApplySorting(filter.SortLabel, filter.SortDirection.Value);
        }
        else
        {
            ApplySorting(nameof(Product.CreatedAt), SortDirection.Descending);
        }
    }
}