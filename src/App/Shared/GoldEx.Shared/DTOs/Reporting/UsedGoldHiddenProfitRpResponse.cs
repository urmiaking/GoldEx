using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Reporting;

public record UsedGoldHiddenProfitRpResponse(
    Guid InvoiceId,
    long InvoiceNumber,
    DateOnly InvoiceDate,
    InvoiceType InvoiceType,
    string CustomerName,
    string Description,
    decimal Weight,
    decimal Fineness,
    decimal FinenessDeductionRate,
    decimal GramPrice,
    int Quantity,
    decimal? ExchangeRate,
    decimal PaidAmount,
    decimal RealValue,
    decimal HiddenProfit,
    string PriceUnitTitle);
