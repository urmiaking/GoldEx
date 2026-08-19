using System;

namespace GoldEx.Shared.DTOs.Reporting;

public record CategorySalesComparisonRpRequest(
    DateTime FromDate1,
    DateTime ToDate1,
    DateTime FromDate2,
    DateTime ToDate2,
    string? Period1Title = null,
    string? Period2Title = null,
    Guid? CategoryId = null,
    string? CategoryTitle = null);
