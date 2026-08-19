namespace GoldEx.Shared.DTOs.Products;

public record ProductImageDto(
    string Url,
    bool IsMain = false,
    int DisplayOrder = 0);
