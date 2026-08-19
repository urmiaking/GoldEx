namespace GoldEx.Shared.DTOs.Vitrine;

public record VitrineCategoryDto(
    Guid Id,
    string Title,
    string PrefixCode,
    int ProductCount);
