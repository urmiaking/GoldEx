using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Offline.Domain.ProductAggregate;

public class Product(
    string name,
    string barcode,
    double weight,
    double? wage,
    ProductType productType,
    WageType? wageType,
    CaratType caratType) : ProductBase(name, barcode, weight, wage, productType, wageType, caratType)
{
    public bool Synced { get; set; }

    public void SetSynced(bool synced) => Synced = synced;
}