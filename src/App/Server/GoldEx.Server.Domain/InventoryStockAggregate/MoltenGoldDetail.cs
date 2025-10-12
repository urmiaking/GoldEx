using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.InventoryStockAggregate;

public class MoltenGoldDetail : EntityBase
{
    public static MoltenGoldDetail Create(decimal weight,
        GoldUnitType weightUnitType,
        string assayNumber,
        decimal fineness,
        CustomerId assayerId)
    {
        return new MoltenGoldDetail(weight, weightUnitType, assayNumber, fineness, assayerId);
    }

    public decimal Weight { get; private set; }
    public GoldUnitType WeightUnitType { get; private set; }
    public decimal Fineness { get; private set; }
    public CustomerId AssayerId { get; private set; }
    public Customer? Assayer { get; private set; }
    public string AssayNumber { get; private set; }

    private MoltenGoldDetail(decimal weight, GoldUnitType weightUnitType, string assayNumber, decimal fineness, CustomerId assayerId)
    {
        Weight = weight;
        WeightUnitType = weightUnitType;
        AssayNumber = assayNumber;
        Fineness = fineness;
        AssayerId = assayerId;
    }

#pragma warning disable CS8618
    private MoltenGoldDetail() { }
#pragma warning restore CS8618
}