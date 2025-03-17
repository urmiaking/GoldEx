using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.Domain.Entities;

namespace GoldEx.Client.Offline.Domain.ProductCategoryAggregate;

public class ProductCategory : ProductCategoryBase, ITrackableEntity
{
    public ProductCategory(string title) : base(title)
    {
    }

    public ProductCategory(ProductCategoryId id, string title) : base(id, title)
    {
    }

    public ModifyStatus Status { get; private set; }
    public void SetStatus(ModifyStatus status) => Status = status;
}