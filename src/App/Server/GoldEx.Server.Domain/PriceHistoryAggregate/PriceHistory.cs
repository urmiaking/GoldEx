using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.PriceAggregate;

namespace GoldEx.Server.Domain.PriceHistoryAggregate;

public class PriceHistory(PriceId priceId, double currentValue, string lastUpdate, string dailyChangeRate, string unit)
    : EntityBase<int>
{
    public double CurrentValue { get; private set; } = currentValue;
    public string LastUpdate { get; private set; } = lastUpdate;
    public string Unit { get; private set; } = unit;
    public string DailyChangeRate { get; private set; } = dailyChangeRate;

    public PriceId PriceId { get; set; } = priceId;
    public Price? Price { get; set; }
}