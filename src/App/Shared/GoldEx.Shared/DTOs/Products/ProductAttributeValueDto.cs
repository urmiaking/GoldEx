using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Products;

public record ProductAttributeValueDto(
    Guid AttributeId,
    string? Title,
    string? Unit,
    string Value,
    decimal? NumericValue = null,
    ProductAttributeDataType DataType = ProductAttributeDataType.Text);
