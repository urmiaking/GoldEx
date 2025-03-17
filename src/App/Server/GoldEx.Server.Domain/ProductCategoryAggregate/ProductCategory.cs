using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.Domain.Entities;

namespace GoldEx.Server.Domain.ProductCategoryAggregate;

public class ProductCategory : ProductCategoryBase, ISoftDeleteEntity
{
    public ProductCategory(string title) : base(title)
    {
    }

    public ProductCategory(ProductCategoryId id, string title) : base(id, title)
    {
    }

    public bool IsDeleted { get; private set; }
    public void SetDeleted() => IsDeleted = true;
}