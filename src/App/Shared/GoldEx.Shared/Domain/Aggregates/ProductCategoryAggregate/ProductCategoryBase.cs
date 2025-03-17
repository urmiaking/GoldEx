using GoldEx.Shared.Domain.Entities;

namespace GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;

public readonly record struct ProductCategoryId(Guid Value);
public class ProductCategoryBase : EntityBase<ProductCategoryId>, ISyncableEntity
{
    public ProductCategoryBase(string title) : base(new ProductCategoryId(Guid.NewGuid()))
    {
        Title = title;
    }

    public ProductCategoryBase(ProductCategoryId id, string title) : base(id)
    {
        Title = title;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private ProductCategoryBase()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        
    }

    public string Title { get; private set; }
    public void SetTitle(string title) => Title = title;

    public DateTime LastModifiedDate { get; private set; }
    public void SetLastModifiedDate() => LastModifiedDate = DateTime.UtcNow;
}