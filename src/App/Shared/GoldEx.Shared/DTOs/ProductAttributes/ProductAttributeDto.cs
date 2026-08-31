using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.ProductAttributes;

public record ProductAttributeDto(
    Guid Id,
    string Title,
    string? Unit,
    ProductAttributeDataType DataType,
    string? Options,
    string? Description);

public record CreateProductAttributeRequest(
    string Title,
    string? Unit,
    ProductAttributeDataType DataType,
    string? Options = null,
    string? Description = null);

public record UpdateProductAttributeRequest(
    string Title,
    string? Unit,
    ProductAttributeDataType DataType,
    string? Options = null,
    string? Description = null);
