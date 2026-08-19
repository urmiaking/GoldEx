using GoldEx.Shared.DTOs.Products;

namespace GoldEx.Shared.DTOs.Vitrine;

public record UpdateProductVitrineRequest(
    bool ShowInVitrine,
    bool IsFeatured,
    string? VitrineDescription,
    List<ProductImageDto>? Images);
