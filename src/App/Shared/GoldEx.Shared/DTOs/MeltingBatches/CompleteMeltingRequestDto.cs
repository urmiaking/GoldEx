namespace GoldEx.Shared.DTOs.MeltingBatches;

public record CompleteMeltingRequestDto(decimal Weight, decimal Fineness, string AssayNumber, string? Description);