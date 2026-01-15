using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Reporting;

public record CoinInventoryRpRequest(
    ItemStatus? ItemStatus,
    Guid? CoinId,
    DateTime? FromDate = null,
    DateTime? ToDate = null);