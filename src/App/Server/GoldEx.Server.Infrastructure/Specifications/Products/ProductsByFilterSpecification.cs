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
        ApplyPaging(Math.Max(0, filter.Skip ?? 0), filter.Take ?? 100);

        AddInclude(x => x.WagePriceUnit!);
        AddInclude(x => x.ProductCategory!);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            AddCriteria(x => x.Name.Contains(filter.Search) || x.Barcode.Contains(filter.Search));
        }

        switch (productFilter.Status)
        {
            case ProductStatus.Available:
                AddCriteria(x => x.InvoiceItem == null);
                if (productFilter.Start.HasValue)
                {
                    AddCriteria(x => x.CreatedAt >= productFilter.Start.Value);
                }
                if (productFilter.End.HasValue)
                {
                    AddCriteria(x => x.CreatedAt <= productFilter.End.Value);
                }
                break;

            case ProductStatus.Sold:
                AddCriteria(x => x.InvoiceItem!.Invoice != null);
                AddInclude(x => x.InvoiceItem!.Invoice!);

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
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(productFilter.Status), "Unsupported product status provided.");
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