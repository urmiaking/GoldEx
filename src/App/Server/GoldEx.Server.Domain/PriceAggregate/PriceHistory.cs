using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.PriceAggregate;

public class PriceHistory : EntityBase
{
    public static PriceHistory Create(decimal currentValue, string lastUpdate, string dailyChangeRate, string unit)
    {
        return new PriceHistory
        {
            CurrentValue = currentValue,
            LastUpdate = lastUpdate,
            DailyChangeRate = dailyChangeRate,
            Unit = unit
        };
    }

#pragma warning disable CS8618
    private PriceHistory() { }
#pragma warning restore CS8618

    public decimal CurrentValue { get; private set; }
    public string LastUpdate { get; private set; }
    public string Unit { get; private set; }
    public string DailyChangeRate { get; private set; }

    public void SetCurrentValue(decimal currentValue) => CurrentValue = currentValue;
    public void SetLastUpdate(string lastUpdate) => LastUpdate = lastUpdate;
    public void SetDailyChangeRate(string dailyChangeRate) => DailyChangeRate = dailyChangeRate;
    public void SetUnit(string unit) => Unit = unit;
}