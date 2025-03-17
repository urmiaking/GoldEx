using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Domain.Aggregates.ProductAggregate;

public readonly record struct ProductId(Guid Value);
public class ProductBase<TCategory> : EntityBase<ProductId>, ISyncableEntity where TCategory : ProductCategoryBase
{
    public ProductBase(string name,
        string barcode,
        double weight,
        double? wage,
        ProductType productType,
        WageType? wageType,
        CaratType caratType,
        ProductCategoryId categoryId) : base(new ProductId(Guid.NewGuid()))
    {
        Name = name;
        Barcode = barcode;
        Weight = weight;
        Wage = wage;
        ProductType = productType;
        CaratType = caratType;
        WageType = wageType;
        ProductCategoryId = categoryId;
    }

    public ProductBase(ProductId id,
        string name,
        string barcode,
        double weight,
        double? wage,
        ProductType productType,
        WageType? wageType,
        CaratType caratType, 
        ProductCategoryId categoryId) : base(id)
    {
        Name = name;
        Barcode = barcode;
        Weight = weight;
        Wage = wage;
        ProductType = productType;
        CaratType = caratType;
        WageType = wageType;
        ProductCategoryId = categoryId;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected ProductBase()
    {
        
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public string Name { get; private set; }
    public string Barcode { get; private set; }
    public double Weight { get; private set; }
    public double? Wage { get; private set; }
    public ProductType ProductType { get; private set; }
    public CaratType CaratType { get; private set; }
    public WageType? WageType { get; private set; }
    public DateTime LastModifiedDate { get; private set; } = DateTime.UtcNow;
    public ProductCategoryId ProductCategoryId { get; private set; }
    public TCategory ProductCategory { get; private set; }

    public void SetName(string name) => Name = name;
    public void SetBarcode(string barcode) => Barcode = barcode;
    public void SetWeight(double weight) => Weight = weight;
    public void SetWage(double? wage) => Wage = wage;
    public void SetProductType(ProductType productType) => ProductType = productType;
    public void SetCaratType(CaratType caratType) => CaratType = caratType;
    public void SetWageType(WageType? wageType) => WageType = wageType;
    public void SetLastModifiedDate() => LastModifiedDate = DateTime.UtcNow;
    public void SetProductCategory(ProductCategoryId categoryId) => ProductCategoryId = categoryId;
}