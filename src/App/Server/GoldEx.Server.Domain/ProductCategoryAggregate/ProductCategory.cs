using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Domain.ProductCategoryAggregate;

public readonly record struct ProductCategoryId(Guid Value);

public class ProductCategory : EntityBase<ProductCategoryId>
{
    public static ProductCategory Create(string title, string prefixCode)
    {
        return new ProductCategory
        {
            Id = new ProductCategoryId(Guid.CreateVersion7()),
            Title = title,
            PrefixCode = prefixCode
        };
    }

#pragma warning disable CS8618
    private ProductCategory() { }
#pragma warning restore CS8618

    public string Title { get; private set; }
    public string PrefixCode { get; private set; }
    public IReadOnlyList<Product>? Products { get; private set; }

    public void SetTitle(string title) => Title = title;

    public void SetPrefixCode(string prefixCode) => PrefixCode = prefixCode;
}