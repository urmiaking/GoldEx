using GoldEx.Sdk.Common.Definitions;

namespace GoldEx.Shared.Attributes;

// Custom attribute
[AttributeUsage(AttributeTargets.Field)]
public class MarketAttribute(MarketType marketType) : Attribute
{
    public MarketType MarketType { get; } = marketType;
}