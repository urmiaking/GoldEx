using GoldEx.Shared.Domain.Aggregates.PriceAggregate;

namespace GoldEx.Client.Offline.Domain.PriceAggregate;

public class PriceHistory(
    double currentValue,
    string lastUpdate,
    string dailyChangeRate,
    string unit) : PriceHistoryBase(currentValue, lastUpdate, dailyChangeRate, unit);