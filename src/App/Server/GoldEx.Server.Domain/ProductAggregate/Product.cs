using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.ProductAggregate;

public class Product : ProductBase<ProductCategory>,
    ISoftDeleteEntity
{

    public Product(string name,
        string barcode,
        double weight,
        double? wage,
        ProductType productType,
        WageType? wageType,
        CaratType caratType,
        Guid createdUserId,
        ProductCategoryId categoryId) : base(name,
        barcode,
        weight,
        wage,
        productType,
        wageType,
        caratType,
        categoryId)
    {
        CreatedUserId = createdUserId;
    }

    public Product(ProductId id,
        string name,
        string barcode,
        double weight,
        double? wage,
        ProductType productType,
        WageType? wageType,
        CaratType caratType,
        Guid createdUserId,
        ProductCategoryId categoryId) : this(name, barcode, weight, wage, productType, wageType, caratType, createdUserId, categoryId)
    {
        Id = id;
    }

    private Product()
    {
        
    }

    public Guid CreatedUserId { get; private set; }
    public AppUser? CreatedUser { get; private set; }
    public bool IsDeleted { get; private set; }
    
    public void SetDeleted() => IsDeleted = true;
    public void SetCreatedUserId(Guid createdUserId) => CreatedUserId = createdUserId;
}