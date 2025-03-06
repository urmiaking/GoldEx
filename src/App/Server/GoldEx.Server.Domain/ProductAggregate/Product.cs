using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.ProductAggregate;

public class Product(string name,
    string barcode,
    double weight,
    double? wage,
    ProductType productType,
    WageType? wageType,
    CaratType caratType,
    Guid createdUserId) : ProductBase(name, barcode, weight, wage, productType, wageType, caratType), ISoftDeleteEntity
{
    public Product(ProductId id,
        string name,
        string barcode,
        double weight,
        double? wage,
        ProductType productType,
        WageType? wageType,
        CaratType caratType,
        Guid createdUserId) : this(name, barcode, weight, wage, productType, wageType, caratType, createdUserId)
    {
        Id = id;
    }

    public Guid CreatedUserId { get; private set; } = createdUserId;
    public AppUser? CreatedUser { get; private set; }
    public bool IsDeleted { get; private set; }
    
    public void SetDeleted() => IsDeleted = true;
    public void SetCreatedUserId(Guid createdUserId) => CreatedUserId = createdUserId;
}