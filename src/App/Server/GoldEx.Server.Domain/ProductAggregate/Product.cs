using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.ProductAggregate;

public readonly record struct ProductId(Guid Value);
public class Product : EntityBase<ProductId>
{ 
    public static Product Create(
        string name,
        string barcode,
        decimal weight,
        decimal wage,
        ProductType productType,
        CaratType caratType,
        WageType wageType,
        ProductCategoryId productCategoryId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(weight, 0, nameof(weight));

        ArgumentOutOfRangeException.ThrowIfLessThan(wage, 0, nameof(wage));

        if (wageType is Shared.Enums.WageType.Percent && wage is > 100)
            throw new ArgumentOutOfRangeException(nameof(wage), "درصد اجرت باید بین 0 الی 100 باشد");

        return new Product
        {
            Id = new ProductId(Guid.NewGuid()),
            Name = name,
            Barcode = barcode,
            Weight = weight,
            Wage = wage,
            ProductType = productType,
            CaratType = caratType,
            WageType = wageType,
            ProductCategoryId = productCategoryId
        };
    }

#pragma warning disable CS8618
    private Product() { }
#pragma warning restore CS8618

    public string Name { get; private set; }
    public string Barcode { get; private set; }
    public decimal Weight { get; private set; }
    public decimal Wage { get; private set; }
    public ProductType ProductType { get; private set; }
    public CaratType CaratType { get; private set; }
    public WageType WageType { get; private set; }
    public ProductCategoryId? ProductCategoryId { get; private set; }
    public ProductCategory? ProductCategory { get; private set; }

    private readonly List<GemStone> _stones = [];
    public IReadOnlyList<GemStone> GemStones => _stones;

    public Product SetName(string name)
    {
        Name = name;
        return this;
    }

    public Product SetBarcode(string barcode)
    {
        Barcode = barcode;
        return this;
    }

    public Product SetWeight(decimal weight)
    {
        Weight = weight;
        return this;
    }

    public Product SetWage(decimal wage)
    {
        Wage = wage;
        return this;
    }

    public Product SetProductType(ProductType productType)
    {
        ProductType = productType;
        return this;
    }

    public Product SetCaratType(CaratType caratType)
    {
        CaratType = caratType;
        return this;
    }

    public Product SetWageType(WageType wageType)
    {
        WageType = wageType;
        return this;
    }

    public Product SetProductCategory(ProductCategoryId? categoryId)
    {
        ProductCategoryId = categoryId;
        return this;
    }

    public Product SetGemStones(IEnumerable<GemStone>? stones)
    {
        _stones.Clear();
        if (stones is not null)
            _stones.AddRange(stones);

        return this;
    }
    public void ClearGemStones() => _stones.Clear();
}