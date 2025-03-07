using GoldEx.Shared.Domain.Aggregates.PriceAggregate;
using GoldEx.Shared.Domain.Entities;

namespace GoldEx.Server.Domain.PriceAggregate;

public class PriceHistory(double currentValue, string lastUpdate, string dailyChangeRate, string unit)
    : PriceHistoryBase(currentValue, lastUpdate, dailyChangeRate, unit);