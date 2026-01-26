using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Reporting;

public record CurrencyInventoryRpRequest(
    ItemStatus? ItemStatus,
    DateTime? FromDate = null,
    DateTime? ToDate = null);