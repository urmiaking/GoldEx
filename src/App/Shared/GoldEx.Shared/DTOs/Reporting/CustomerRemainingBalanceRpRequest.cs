using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Reporting;

public record CustomerRemainingBalanceRpRequest(DateTime? UntilDate,
    Guid? PriceUnitId,
    decimal? MinimumThreshold,
    string? SearchQuery,
    TransactionType? TransactionType
);