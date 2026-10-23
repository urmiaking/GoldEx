using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Reporting;

public record ProfitAnalysisRpRequest(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    Guid? CustomerId = null,
    Guid? CategoryId = null,
    ProductType? ProductType = null,
    bool SummaryOnly = false);
