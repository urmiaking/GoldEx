using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.ProductAttributeAggregate;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Domain.ProductCategoryAggregate;

public class CategoryAttribute : EntityBase, IStoreFiltered
{
    public StoreId StoreId { get; private set; }

    public ProductCategoryId ProductCategoryId { get; private set; }
    public ProductCategory? ProductCategory { get; private set; }

    public ProductAttributeId ProductAttributeId { get; private set; }
    public ProductAttribute? ProductAttribute { get; private set; }

    public bool IsRequired { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool ShowInFilter { get; private set; }

#pragma warning disable CS8618
    private CategoryAttribute() { }
#pragma warning restore CS8618

    public static CategoryAttribute Create(
        ProductCategoryId categoryId,
        ProductAttributeId attributeId,
        bool isRequired = false,
        int displayOrder = 0,
        bool showInFilter = true,
        StoreId storeId = default)
    {
        return new CategoryAttribute
        {
            ProductCategoryId = categoryId,
            ProductAttributeId = attributeId,
            IsRequired = isRequired,
            DisplayOrder = displayOrder,
            ShowInFilter = showInFilter,
            StoreId = storeId
        };
    }

    public void Update(bool isRequired, int displayOrder, bool showInFilter)
    {
        IsRequired = isRequired;
        DisplayOrder = displayOrder;
        ShowInFilter = showInFilter;
    }
}
