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
        double weight,
        double? wage,
        ProductType productType,
        CaratType caratType,
        WageType? wageType,
        ProductCategoryId productCategoryId)
    {
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
    public double Weight { get; private set; }
    public double? Wage { get; private set; }
    public ProductType ProductType { get; private set; }
    public CaratType CaratType { get; private set; }
    public WageType? WageType { get; private set; }
    public ProductCategoryId ProductCategoryId { get; private set; }
    public ProductCategory ProductCategory { get; private set; } = null!;

    private readonly List<GemStone> _stones = [];
    public IReadOnlyList<GemStone> GemStones => _stones;

    public void SetName(string name) => Name = name;
    public void SetBarcode(string barcode) => Barcode = barcode;
    public void SetWeight(double weight) => Weight = weight;
    public void SetWage(double? wage) => Wage = wage;
    public void SetProductType(ProductType productType) => ProductType = productType;
    public void SetCaratType(CaratType caratType) => CaratType = caratType;
    public void SetWageType(WageType? wageType) => WageType = wageType;
    public void SetProductCategory(ProductCategoryId categoryId) => ProductCategoryId = categoryId;
    public void SetGemStones(List<GemStone> stones)
    {
        _stones.Clear();
        _stones.AddRange(stones);
    }
    public void ClearGemStones() => _stones.Clear();
}