namespace GoldEx.Shared.DTOs.Reporting;

public record UsedGoldHiddenProfitRpRequest(
    Guid? CustomerId = null,
    Guid? PriceUnitId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null);
