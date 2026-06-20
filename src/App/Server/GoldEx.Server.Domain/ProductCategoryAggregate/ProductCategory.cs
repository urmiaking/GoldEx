using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Domain.ProductCategoryAggregate;

public readonly record struct ProductCategoryId(Guid Value);

public class ProductCategory : EntityBase<ProductCategoryId>, IStoreFiltered
{
    public static ProductCategory Create(string title, string prefixCode, StoreId storeId = default)
    {
        return new ProductCategory
        {
            Id = new ProductCategoryId(Guid.CreateVersion7()),
            Title = title,
            PrefixCode = prefixCode,
            StoreId = storeId
        };
    }

#pragma warning disable CS8618
    private ProductCategory() { }
#pragma warning restore CS8618

    public StoreId StoreId { get; private set; }
    public string Title { get; private set; }
    public string PrefixCode { get; private set; }
    public IReadOnlyList<Product>? Products { get; private set; }

    public void SetTitle(string title) => Title = title;

    public void SetPrefixCode(string prefixCode) => PrefixCode = prefixCode;
}