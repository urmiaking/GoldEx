using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Shared.DTOs.MoltenGold;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.MoltenGoldTradeAggregate;

public class MoltenGoldTrade : EntityBase<Guid>, IStoreFiltered
{
    private MoltenGoldTrade() { }

    public StoreId StoreId { get; private set; }
    public InvoiceType Side { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public ProductId? ProductId { get; private set; }
    public PriceUnitId PriceUnitId { get; private set; }
    public decimal ExchangeRate { get; private set; }
    public decimal Weight { get; private set; }
    public decimal RemainingWeight { get; private set; }
    public decimal Fineness { get; private set; }
    public MoltenGoldRateBasis RateBasis { get; private set; }
    public decimal Rate { get; private set; }
    public decimal GramRate { get; private set; }
    public decimal CommissionPercent { get; private set; }
    public string? AssayNumber { get; private set; }
    public bool IsCancelled { get; private set; }
    public bool IsPaper { get; private set; }
    public PriceUnitId? GoldPriceUnitId { get; private set; }
    public decimal RemainingGoldBookAmount { get; private set; }
    public decimal RemainingGoldWeight { get; private set; }

    public static MoltenGoldTrade Create(CreateMoltenGoldTrade request, PriceUnitId unit, decimal exchangeRate,
        decimal gramRate, decimal commission, StoreId storeId, PriceUnitId? goldUnit = null)
    {
        var rawAmount = Math.Round(MoltenGoldTradePricing.Value(request.Weight, request.Fineness, gramRate),
            10, MidpointRounding.AwayFromZero);
        return new()
        {
            Id = request.Id, StoreId = storeId, Side = request.Side, CustomerId = new(request.CustomerId),
            ProductId = !request.IsPaper && request.ProductId is Guid product ? new ProductId(product) : null, PriceUnitId = unit,
            ExchangeRate = exchangeRate, Weight = request.Weight, RemainingWeight = request.Weight,
            Fineness = request.Fineness, RateBasis = request.RateBasis, Rate = request.Rate, GramRate = gramRate,
            CommissionPercent = commission, AssayNumber = request.AssayNumber?.Trim(), IsPaper = request.IsPaper,
            GoldPriceUnitId = goldUnit,
            RemainingGoldWeight = request.IsPaper ? Math.Round(request.Weight * request.Fineness / 750m, 10, MidpointRounding.AwayFromZero) : 0,
            RemainingGoldBookAmount = request.IsPaper ? Math.Round(rawAmount * exchangeRate, 10, MidpointRounding.AwayFromZero) : 0
        };
    }

    public decimal Value(decimal weight)
    {
        var value = MoltenGoldTradePricing.Value(weight, Fineness, GramRate);
        return Math.Round(value + MoltenGoldTradePricing.Commission(value, CommissionPercent), 10, MidpointRounding.AwayFromZero);
    }

    public void Deliver(decimal weight)
    {
        if (IsCancelled || weight <= 0 || weight > RemainingWeight)
            throw new InvalidOperationException("وزن این نوبت از ماندهٔ قابل تحویل بیشتر است.");

        RemainingWeight -= weight;
    }

    public MoltenGoldPosition TakeGold(decimal weight)
    {
        if (weight <= 0 || weight > RemainingWeight)
            throw new InvalidOperationException("وزن از ماندهٔ قابل تحویل بیشتر است.");
        var goldWeight = weight == RemainingWeight ? RemainingGoldWeight
            : Math.Round(weight * Fineness / 750m, 10, MidpointRounding.AwayFromZero);
        var position = TakeGoldPosition(goldWeight);
        Deliver(weight);
        return position;
    }

    public MoltenGoldPosition SettleGold(decimal goldWeight)
    {
        var position = TakeGoldPosition(goldWeight);
        RemainingWeight = Math.Round(RemainingGoldWeight * 750m / Fineness, 10, MidpointRounding.AwayFromZero);
        return position;
    }

    private MoltenGoldPosition TakeGoldPosition(decimal goldWeight)
    {
        if (!IsPaper || IsCancelled || goldWeight <= 0 || goldWeight > RemainingGoldWeight)
            throw new InvalidOperationException("وزن از ماندهٔ طلایی معامله بیشتر است.");
        var bookAmount = goldWeight == RemainingGoldWeight ? RemainingGoldBookAmount
            : Math.Round(RemainingGoldBookAmount * goldWeight / RemainingGoldWeight, 10, MidpointRounding.AwayFromZero);
        RemainingGoldWeight -= goldWeight;
        RemainingGoldBookAmount -= bookAmount;
        return new(goldWeight, bookAmount);
    }

    public void Cancel() => IsCancelled = true;
}

public record MoltenGoldPosition(decimal Weight, decimal BookAmount);
