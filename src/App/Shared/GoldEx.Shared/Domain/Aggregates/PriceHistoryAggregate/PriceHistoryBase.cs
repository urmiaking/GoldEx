using GoldEx.Shared.Domain.Aggregates.PriceAggregate;
using GoldEx.Shared.Domain.Entities;

namespace GoldEx.Shared.Domain.Aggregates.PriceHistoryAggregate;

public class PriceHistoryBase : EntityBase<int>
{
    public PriceHistoryBase(PriceId priceId, double currentValue, string lastUpdate, string dailyChangeRate, string unit)
    {
        CurrentValue = currentValue;
        LastUpdate = lastUpdate;
        Unit = unit;
        DailyChangeRate = dailyChangeRate;
        PriceId = priceId;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected PriceHistoryBase() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public double CurrentValue { get; private set; }
    public string LastUpdate { get; private set; }
    public string Unit { get; private set; }
    public string DailyChangeRate { get; private set; }

    public PriceId PriceId { get; set; }
}