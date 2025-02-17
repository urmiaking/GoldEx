using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.PriceAggregate;

public class PriceHistory : EntityBase<int>
{
    public double CurrentValue { get; private set; }
    public string LastUpdate { get; private set; }
    public string DailyChangeRate { get; private set; }

    public PriceHistory(double currentValue, string lastUpdate, string dailyChangeRate)
    {
        CurrentValue = currentValue;
        LastUpdate = lastUpdate;
        DailyChangeRate = dailyChangeRate;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private PriceHistory() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}