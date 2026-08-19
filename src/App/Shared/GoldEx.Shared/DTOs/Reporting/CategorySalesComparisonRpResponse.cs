using System;

namespace GoldEx.Shared.DTOs.Reporting;

public record CategorySalesComparisonRpResponse(
    Guid? CategoryId,
    string CategoryTitle,
    decimal Weight1,
    decimal Weight2,
    decimal WeightDeltaPercent,
    int Quantity1,
    int Quantity2,
    decimal QuantityDeltaPercent,
    decimal Amount1,
    decimal Amount2,
    decimal AmountDeltaPercent,
    decimal Profit1,
    decimal Profit2,
    decimal ProfitDeltaPercent);
