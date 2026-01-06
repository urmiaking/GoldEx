using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Reporting;

public record CustomerTransactionRpRequest(Guid CustomerId,
    Guid? PriceUnitId,
    LedgerAccountRole? LedgerRole,
    DateTime? FromDate = null,
    DateTime? ToDate = null);