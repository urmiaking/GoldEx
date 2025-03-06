using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Offline.Domain.ProductAggregate;

public class Product(
    string name,
    string barcode,
    double weight,
    double? wage,
    ProductType productType,
    WageType? wageType,
    CaratType caratType) : ProductBase(name, barcode, weight, wage, productType, wageType, caratType), ITrackableEntity
{
    public Product(ProductId id,
        string name,
        string barcode,
        double weight,
        double? wage,
        ProductType productType,
        WageType? wageType,
        CaratType caratType) : this(name, barcode, weight, wage, productType, wageType, caratType)
    {
        Id = id;
    }

    public ModifyStatus Status { get; private set; } = ModifyStatus.Created;

    public void SetStatus(ModifyStatus status) => Status = status;
}