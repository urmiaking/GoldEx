using GoldEx.Shared.Domain.Aggregates.PriceAggregate;
using GoldEx.Shared.Domain.Aggregates.PriceHistoryAggregate;

namespace GoldEx.Client.Offline.Domain.PriceHistoryAggregate;

public class PriceHistory(
    PriceId priceId,
    double currentValue,
    string lastUpdate,
    string dailyChangeRate,
    string unit) : PriceHistoryBase(priceId,
    currentValue,
    lastUpdate,
    dailyChangeRate,
    unit);