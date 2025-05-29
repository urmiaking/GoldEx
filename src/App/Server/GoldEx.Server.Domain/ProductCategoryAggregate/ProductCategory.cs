using GoldEx.Sdk.Server.Domain.Entities;
namespace GoldEx.Server.Domain.ProductCategoryAggregate;

public readonly record struct ProductCategoryId(Guid Value);

public class ProductCategory : EntityBase<ProductCategoryId>
{
    public static ProductCategory Create(string title)
    {
        return new ProductCategory
        {
            Id = new ProductCategoryId(Guid.NewGuid()),
            Title = title
        };
    }

#pragma warning disable CS8618
    private ProductCategory() { }
#pragma warning restore CS8618

    public string Title { get; private set; }
    public void SetTitle(string title) => Title = title;
}