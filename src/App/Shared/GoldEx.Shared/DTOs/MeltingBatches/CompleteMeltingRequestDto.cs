namespace GoldEx.Shared.DTOs.MeltingBatches;

public record CompleteMeltingRequestDto(decimal Weight, decimal Fineness, string AssayNumber, decimal GramPrice, Guid PriceUnitId, string? Description);