using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Vitrine;

public record VitrineAttributeValueDto(
    Guid AttributeId,
    string Title,
    string? Unit,
    string Value,
    decimal? NumericValue = null,
    ProductAttributeDataType DataType = ProductAttributeDataType.Text,
    int DisplayOrder = 0);
