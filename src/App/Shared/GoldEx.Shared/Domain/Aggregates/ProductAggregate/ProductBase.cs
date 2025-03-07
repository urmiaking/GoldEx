using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Domain.Aggregates.ProductAggregate;

public readonly record struct ProductId(Guid Value);
public class ProductBase : EntityBase<ProductId>, ISyncableEntity
{
    public ProductBase(string name,
        string barcode,
        double weight,
        double? wage,
        ProductType productType,
        WageType? wageType,
        CaratType caratType) : base(new ProductId(Guid.NewGuid()))
    {
        Name = name;
        Barcode = barcode;
        Weight = weight;
        Wage = wage;
        ProductType = productType;
        CaratType = caratType;
        WageType = wageType;
    }

    public ProductBase(ProductId id,
        string name,
        string barcode,
        double weight,
        double? wage,
        ProductType productType,
        WageType? wageType,
        CaratType caratType) : base(id)
    {
        Name = name;
        Barcode = barcode;
        Weight = weight;
        Wage = wage;
        ProductType = productType;
        CaratType = caratType;
        WageType = wageType;
    }

    public string Name { get; private set; }
    public string Barcode { get; private set; }
    public double Weight { get; private set; }
    public double? Wage { get; private set; }
    public ProductType ProductType { get; private set; }
    public CaratType CaratType { get; private set; }
    public WageType? WageType { get; private set; }
    public DateTime LastModifiedDate { get; private set; } = DateTime.UtcNow;

    public void SetName(string name) => Name = name;
    public void SetBarcode(string barcode) => Barcode = barcode;
    public void SetWeight(double weight) => Weight = weight;
    public void SetWage(double? wage) => Wage = wage;
    public void SetProductType(ProductType productType) => ProductType = productType;
    public void SetCaratType(CaratType caratType) => CaratType = caratType;
    public void SetWageType(WageType? wageType) => WageType = wageType;
    public void SetLastModifiedDate() => LastModifiedDate = DateTime.UtcNow;
}