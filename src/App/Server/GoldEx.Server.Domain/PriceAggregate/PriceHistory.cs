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
    protected PriceHistory() { }
#pragma warning restore CS8618

    public decimal CurrentValue { get; private set; }
    public string LastUpdate { get; private set; }
    public string Unit { get; private set; }
    public string DailyChangeRate { get; private set; }
    public DateTime DateTime { get; private set; } = DateTime.Now;
    public DateTime ExpireDate { get; private set; }

    public void UpdateExpireDate(DateTime expireDate)
    {
        if (expireDate < DateTime.UtcNow)
            throw new ArgumentOutOfRangeException(nameof(expireDate), "Expire date cannot be in the past.");

        ExpireDate = expireDate;
    }
}