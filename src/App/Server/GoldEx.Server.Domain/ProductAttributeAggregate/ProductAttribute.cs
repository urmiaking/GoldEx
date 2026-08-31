using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.ProductAttributeAggregate;

public readonly record struct ProductAttributeId(Guid Value);

public class ProductAttribute : EntityBase<ProductAttributeId>, IStoreFiltered
{
    public StoreId StoreId { get; private set; }
    public string Title { get; private set; }
    public string? Unit { get; private set; }
    public ProductAttributeDataType DataType { get; private set; }
    public string? Options { get; private set; }
    public string? Description { get; private set; }

#pragma warning disable CS8618
    private ProductAttribute() { }
#pragma warning restore CS8618

    private ProductAttribute(
        string title,
        string? unit,
        ProductAttributeDataType dataType,
        string? options,
        string? description,
        StoreId storeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        Id = new ProductAttributeId(Guid.CreateVersion7());
        Title = title.Trim();
        Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();
        DataType = dataType;
        Options = string.IsNullOrWhiteSpace(options) ? null : options.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        StoreId = storeId;
    }

    public static ProductAttribute Create(
        string title,
        string? unit,
        ProductAttributeDataType dataType,
        string? options = null,
        string? description = null,
        StoreId storeId = default)
    {
        return new ProductAttribute(title, unit, dataType, options, description, storeId);
    }

    public void Update(
        string title,
        string? unit,
        ProductAttributeDataType dataType,
        string? options = null,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        Title = title.Trim();
        Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();
        DataType = dataType;
        Options = string.IsNullOrWhiteSpace(options) ? null : options.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}
