using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Products;

public record ProductFilter(ItemStatus Status, DateTime? Start, DateTime? End);