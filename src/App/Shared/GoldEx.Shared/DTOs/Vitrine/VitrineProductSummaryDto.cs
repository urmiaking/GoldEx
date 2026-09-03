using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Vitrine;

public record VitrineProductSummaryDto(
    Guid Id,
    string Barcode,
    string Name,
    decimal Weight,
    decimal Fineness,
    ProductType ProductType,
    Guid? CategoryId,
    string? CategoryTitle,
    string? MainImageUrl,
    decimal EstimatedPrice,
    bool IsFeatured,
    bool IsAvailable = true,
    IReadOnlyList<VitrineAttributeValueDto>? Attributes = null,
    decimal Wage = 0,
    WageType? WageType = null,
    decimal WageAmount = 0);
