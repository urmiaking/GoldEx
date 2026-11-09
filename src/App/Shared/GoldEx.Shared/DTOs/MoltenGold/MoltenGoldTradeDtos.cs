using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;

namespace GoldEx.Shared.DTOs.MoltenGold;

public enum MoltenGoldRateBasis { Gram, Mesghal }
public enum MoltenGoldTradeFilter { Today, Unsettled, Undelivered, All }
public enum MoltenGoldTradeEventKind { Delivery, Payment, Transfer, Agreement, GoldSettlement }

public record CreateMoltenGoldTrade(Guid Id, InvoiceType Side, Guid CustomerId, Guid? ProductId,
    decimal Weight, decimal Fineness, MoltenGoldRateBasis RateBasis, decimal Rate,
    decimal? CommissionPercent, string? AssayNumber, decimal DeliveryWeight, Guid? FinancialAccountId,
    decimal PaymentAmount, string? Reference, bool IsPaper = false);

public record DeliverMoltenGold(Guid OperationId, decimal Weight, string? AssayNumber = null, Guid? ProductId = null);
public record SettleMoltenGold(Guid OperationId, Guid OtherTradeId, decimal Weight);
public record MoltenGoldSettlementChoice(Guid Id, string CustomerName, decimal RemainingGoldWeight, decimal GramRate, DateTime Date);
public record MoltenGoldPaymentChoice(Guid Id, DateTime Date, decimal RemainingAmount);
public record PayMoltenGold(Guid OperationId, Guid AccountId, decimal Amount, string? Reference);
public record TransferMoltenGold(Guid OperationId, Guid InvoiceId, Guid OtherCustomerId, Guid? OtherInvoiceId,
    decimal Amount, string? Reference, Guid? OtherTradeId = null);
public record MoltenGoldStockChoice(Guid Id, string Name, string Barcode, decimal Fineness, decimal Available);
public record MoltenGoldDeskSettings(decimal CommissionPercent, decimal GramPerMesghal, Guid PriceUnitId,
    Guid GoldPriceUnitId = default, decimal DayRate = 0, DateTime? RateUpdatedAt = null);
public record MoltenGoldTradeEventResponse(MoltenGoldTradeEventKind Kind, DateTime Date, decimal Amount,
    decimal Weight, Guid? InvoiceId, long? InvoiceNumber, decimal InvoiceRemaining, string? Reference);
public record MoltenGoldTradeResponse(Guid Id, InvoiceType Side, Guid CustomerId, string CustomerName,
    DateTime Date, decimal Weight, decimal Fineness, decimal CommissionPercent, decimal Total,
    decimal RemainingWeight, decimal RemainingAmount, List<MoltenGoldTradeEventResponse> Events, bool IsPaper = false,
    decimal RemainingGoldWeight = 0);

public static class MoltenGoldTradePricing
{
    public static decimal GramRate(decimal rate, MoltenGoldRateBasis basis, decimal gramPerMesghal)
        => basis == MoltenGoldRateBasis.Mesghal ? rate * 750m / (gramPerMesghal * 705m) : rate;

    public static decimal Value(decimal weight, decimal fineness, decimal gramRate)
        => CalculatorHelper.Product.CalculateRawPrice(weight, gramRate, fineness, 1, ProductType.MoltenGold);

    public static decimal Commission(decimal value, decimal percent)
        => CalculatorHelper.Product.CalculateWage(value, 1, percent, WageType.Percent, null);
}
