using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.ProductCategories;

public record CategoryAttributeDto(
    Guid AttributeId,
    string Title,
    string? Unit,
    ProductAttributeDataType DataType,
    string? Options,
    bool IsRequired,
    int DisplayOrder,
    bool ShowInFilter = true);

public record CategoryAttributeItemRequest(
    Guid AttributeId,
    bool IsRequired = false,
    int DisplayOrder = 0,
    bool ShowInFilter = true);

public record SetCategoryAttributesRequest(
    Guid CategoryId,
    List<CategoryAttributeItemRequest> Attributes);
