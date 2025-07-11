using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.Products;

public class ProductsByFilterSpecification : SpecificationBase<Product>
{
    public ProductsByFilterSpecification(RequestFilter filter, ProductFilter productFilter)
    {
        if (filter.Skip < 0)
            filter.Skip = 0;

        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        ApplyPaging(skip, take);

        AddInclude(x => x.WagePriceUnit!);
        AddInclude(x => x.ProductCategory!);

        if (!string.IsNullOrEmpty(filter.Search))
        {
            AddCriteria(x => x.Name.Contains(filter.Search) || x.Barcode.Contains(filter.Search));
        }

        AddCriteria(x => x.ProductStatus == productFilter.Status);

        if (productFilter.Status == ProductStatus.Sold)
        {
            AddInclude(x => x.InvoiceItem!.Invoice!);
            AddCriteria(x => x.InvoiceItem != null && x.InvoiceItem.Invoice != null);

            if (productFilter.Start.HasValue)
            {
                var startDateOnly = DateOnly.FromDateTime(productFilter.Start.Value.Date);
                AddCriteria(x => x.InvoiceItem!.Invoice!.InvoiceDate >= startDateOnly);
            }
            if (productFilter.End.HasValue)
            {
                var endDateOnly = DateOnly.FromDateTime(productFilter.End.Value.Date);
                AddCriteria(x => x.InvoiceItem!.Invoice!.InvoiceDate <= endDateOnly);
            }
        }
        else 
        {
            if (productFilter.Start.HasValue)
            {
                AddCriteria(x => x.CreatedAt >= productFilter.Start.Value);
            }
            if (productFilter.End.HasValue)
            {
                AddCriteria(x => x.CreatedAt <= productFilter.End.Value);
            }
        }

        if (!string.IsNullOrEmpty(filter.SortLabel) && filter.SortDirection.HasValue && filter.SortDirection != SortDirection.None)
        {
            ApplySorting(filter.SortLabel, filter.SortDirection.Value);
        }
        else
        {
            // Default sort order
            ApplySorting(nameof(Product.CreatedAt), SortDirection.Descending);
        }
    }
}