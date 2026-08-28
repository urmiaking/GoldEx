using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductAttributeAggregate;
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

    private readonly List<CategoryAttribute> _attributes = [];
    public IReadOnlyList<CategoryAttribute> Attributes => _attributes;

    public void SetTitle(string title) => Title = title;

    public void SetPrefixCode(string prefixCode) => PrefixCode = prefixCode;

    public void UpdateAttributes(IEnumerable<(ProductAttributeId AttributeId, bool IsRequired, int DisplayOrder, bool ShowInFilter)> desiredAttributes, StoreId storeId = default)
    {
        var desiredList = desiredAttributes.ToList();
        var desiredAttrIds = desiredList.Select(x => x.AttributeId).ToHashSet();

        // 1. Remove attributes that are no longer assigned
        _attributes.RemoveAll(x => !desiredAttrIds.Contains(x.ProductAttributeId));

        // 2. Update existing or add newly assigned
        foreach (var desired in desiredList)
        {
            var existing = _attributes.FirstOrDefault(x => x.ProductAttributeId == desired.AttributeId);
            if (existing != null)
            {
                existing.Update(desired.IsRequired, desired.DisplayOrder, desired.ShowInFilter);
            }
            else
            {
                _attributes.Add(CategoryAttribute.Create(Id, desired.AttributeId, desired.IsRequired, desired.DisplayOrder, desired.ShowInFilter, storeId));
            }
        }
    }

    public void ClearAttributes() => _attributes.Clear();
}