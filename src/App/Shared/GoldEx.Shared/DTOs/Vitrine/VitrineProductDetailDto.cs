using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Vitrine;

public record VitrineProductDetailDto(
    Guid Id,
    string Barcode,
    string Name,
    decimal Weight,
    decimal Wage,
    WageType? WageType,
    decimal Fineness,
    ProductType ProductType,
    Guid? CategoryId,
    string? CategoryTitle,
    string? Description,
    IReadOnlyList<string> ImageUrls,
    IReadOnlyList<VitrineGemStoneDto> GemStones,
    decimal EstimatedPrice,
    decimal RawGoldPrice,
    decimal WageAmount,
    decimal ProfitAmount,
    decimal TaxAmount,
    decimal GramPrice750,
    DateTime UpdatedAt,
    bool IsAvailable = true);
