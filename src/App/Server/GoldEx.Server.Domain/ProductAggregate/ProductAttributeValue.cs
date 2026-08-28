using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.ProductAttributeAggregate;

namespace GoldEx.Server.Domain.ProductAggregate;

public class ProductAttributeValue : EntityBase
{
    public ProductId ProductId { get; private set; }
    public ProductAttributeId AttributeId { get; private set; }
    public ProductAttribute? Attribute { get; private set; }

    public string Value { get; private set; }
    public decimal? NumericValue { get; private set; }

#pragma warning disable CS8618
    private ProductAttributeValue() { }
#pragma warning restore CS8618

    public static ProductAttributeValue Create(
        ProductAttributeId attributeId,
        string value,
        decimal? numericValue = null)
    {
        return new ProductAttributeValue
        {
            AttributeId = attributeId,
            Value = value?.Trim() ?? string.Empty,
            NumericValue = numericValue
        };
    }

    public void Update(string value, decimal? numericValue = null)
    {
        Value = value?.Trim() ?? string.Empty;
        NumericValue = numericValue;
    }
}
