using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Reporting;

public record ProductInventoryRpRequest(
    ItemStatus? ItemStatus,
    ItemType? ItemType,
    Guid? ProductCategoryId,
    DateTime? FromDate = null,
    DateTime? ToDate = null);