using GoldEx.Shared.Domain.Entities;

namespace GoldEx.Shared.Domain.Aggregates.ProductAggregate;

public readonly record struct GemStoneId(Guid Value);
public class GemStoneBase : EntityBase<GemStoneId>
{
    public string Type { get; private set; }
    public string Color { get; private set; }
    public string? Cut { get; private set; }
    public double Carat { get; private set; }
    public string? Purity { get; private set; }

    public GemStoneBase(string type, string color, string? cut, double carat, string? purity) : base(new GemStoneId(Guid.NewGuid()))
    {
        Type = type;
        Color = color;
        Cut = cut;
        Carat = carat;
        Purity = purity;
    }

    public GemStoneBase(GemStoneId id, string type, string color, string? cut, double carat, string? purity) : base(id)
    {
        Type = type;
        Color = color;
        Cut = cut;
        Carat = carat;
        Purity = purity;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected GemStoneBase()
    { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}