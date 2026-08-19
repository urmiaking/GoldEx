namespace GoldEx.Shared.DTOs.Reporting;

public record CategorySalesRpRequest(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    Guid? CategoryId = null,
    string? CategoryTitle = null,
    Guid? PriceUnitId = null);
