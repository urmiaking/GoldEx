namespace GoldEx.Shared.DTOs.Reporting;

public record SoldProductItemRpRequest(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    Guid? CategoryId = null,
    string? CategoryTitle = null,
    string? SearchQuery = null,
    int Skip = 0,
    int Take = 50);
