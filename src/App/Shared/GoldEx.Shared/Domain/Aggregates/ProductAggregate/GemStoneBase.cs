using GoldEx.Shared.Domain.Entities;

namespace GoldEx.Shared.Domain.Aggregates.ProductAggregate;

public class GemStoneBase : EntityBase
{
    public string Code { get; private set; }
    public string Type { get; private set; }
    public string Color { get; private set; }
    public string? Cut { get; private set; }
    public double Carat { get; private set; }
    public string? Purity { get; private set; }

    public ProductId ProductId { get; private set; }

    public GemStoneBase(string code, string type, string color, string? cut, double carat, string? purity)
    {
        Code = code;
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