using GoldEx.Shared.Domain.Aggregates.ProductAggregate;

namespace GoldEx.Server.Domain.ProductAggregate;

public class GemStone : GemStoneBase
{
    public GemStone(string code, string type, string color, string? cut, double carat, string? purity)
        : base(code, type, color, cut, carat, purity)
    {
    }

    private GemStone()
    {
    }
}