﻿using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Domain.Aggregates.ProductAggregate;

public readonly record struct ProductId(Guid Value);
public class ProductBase(
    string name,
    string barcode,
    double weight,
    double? wage,
    ProductType productType,
    WageType? wageType,
    CaratType caratType)
    : SyncableEntityBase<ProductId>(new ProductId(Guid.NewGuid()))
{
    public string Name { get; private set; } = name;
    public string Barcode { get; private set; } = barcode;
    public double Weight { get; private set; } = weight;
    public double? Wage { get; private set; } = wage;
    public ProductType ProductType { get; private set; } = productType;
    public CaratType CaratType { get; private set; } = caratType;
    public WageType? WageType { get; private set; } = wageType;
    public DateTime CreatedAt { get; private set; } = DateTime.Now;

    public void SetName(string name) => Name = name;
    public void SetBarcode(string barcode) => Barcode = barcode;
    public void SetWeight(double weight) => Weight = weight;
    public void SetWage(double? wage) => Wage = wage;
    public void SetProductType(ProductType productType) => ProductType = productType;
    public void SetCaratType(CaratType caratType) => CaratType = caratType;
    public void SetWageType(WageType? wageType) => WageType = wageType;
}