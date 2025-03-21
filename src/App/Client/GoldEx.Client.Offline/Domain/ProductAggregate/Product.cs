using GoldEx.Client.Offline.Domain.ProductCategoryAggregate;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Offline.Domain.ProductAggregate;

public class Product : ProductBase<ProductCategory, GemStone>, ITrackableEntity
{
    public Product(ProductId id,
        string name,
        string barcode,
        double weight,
        double? wage,
        ProductType productType,
        WageType? wageType,
        CaratType caratType,
        ProductCategoryId categoryId) : this(name, barcode, weight, wage, productType, wageType, caratType, categoryId)
    {
        Id = id;
    }

    public Product(string name,
        string barcode,
        double weight,
        double? wage,
        ProductType productType,
        WageType? wageType,
        CaratType caratType,
        ProductCategoryId categoryId) : base(name, barcode, weight, wage, productType, wageType, caratType, categoryId)
    {
    }

    private Product() 
    {
        
    }

    public ModifyStatus Status { get; private set; } = ModifyStatus.Created;

    public void SetStatus(ModifyStatus status) => Status = status;
}