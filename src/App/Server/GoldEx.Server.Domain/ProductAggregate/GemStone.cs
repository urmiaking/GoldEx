using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.ProductAggregate;

public class GemStone : EntityBase
{
    public static GemStone Create(string code, string type, string color, string? cut, decimal carat, string? purity, ProductId productId)
    {
        return new GemStone
        {
            Code = code,
            Type = type,
            Color = color,
            Cut = cut,
            Carat = carat,
            Purity = purity,
            ProductId = productId
        };
    }

#pragma warning disable CS8618 
    private GemStone() { }
#pragma warning restore CS8618

    public string Code { get; private set; }
    public string Type { get; private set; }
    public string Color { get; private set; }
    public string? Cut { get; private set; }
    public decimal Carat { get; private set; }
    public string? Purity { get; private set; }

    public ProductId ProductId { get; private set; }
    public Product? Product { get; private set; }
}