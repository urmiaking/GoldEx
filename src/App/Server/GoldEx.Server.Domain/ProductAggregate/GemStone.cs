using GoldEx.Shared.Domain.Aggregates.ProductAggregate;

namespace GoldEx.Server.Domain.ProductAggregate;

public class GemStone : GemStoneBase
{
    public GemStone(string type, string color, string? cut, double carat, string? purity) : base(type, color, cut, carat, purity)
    {
    }

    public GemStone(GemStoneId id, string type, string color, string? cut, double carat, string? purity) : base(id, type, color, cut, carat, purity)
    {
    }

    private GemStone()
    {
    }
}