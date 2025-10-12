using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.MeltingBatches;

public record MeltingBatchFilter(MeltingBatchStatus? Status, DateTime? Start, DateTime? End);