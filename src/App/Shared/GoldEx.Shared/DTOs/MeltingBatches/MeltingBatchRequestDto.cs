namespace GoldEx.Shared.DTOs.MeltingBatches;

public record MeltingBatchRequestDto(string Description, List<Guid> ProductIds);