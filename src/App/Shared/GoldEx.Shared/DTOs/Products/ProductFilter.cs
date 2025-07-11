using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Products;

public record ProductFilter(ProductStatus Status, DateTime? Start, DateTime? End);