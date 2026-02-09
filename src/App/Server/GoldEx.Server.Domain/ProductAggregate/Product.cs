using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.ProductAggregate;

public readonly record struct ProductId(Guid Value);
public class Product : EntityBase<ProductId>
{
    public static Product CreateBrokenProduct(
        string name,
        decimal weight,
        decimal fineness,
        GoldUnitType goldUnitType)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(weight, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(fineness, 0);

        return new Product
        {
            Id = new ProductId(Guid.CreateVersion7()),
            Name = name,
            Weight = weight,
            Fineness = fineness,
            GoldUnitType = goldUnitType,
            ProductType = ProductType.UsedGold
        };
    }

    public static Product CreateMoltenGold(
        string name,
        decimal weight,
        decimal wage,
        decimal fineness,
        GoldUnitType goldUnitType,
        WageType? wageType,
        PriceUnitId? wagePriceUnitId,
        ProductCategoryId? productCategoryId,
        MoltenGold? moltenGold)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(weight, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(wage, 0);

        if (wageType is Shared.Enums.WageType.Percent && wage > 100)
            throw new ArgumentOutOfRangeException(nameof(wage), "درصد اجرت باید بین 0 الی 100 باشد");

        return new Product
        {
            Id = new ProductId(Guid.CreateVersion7()),
            Name = name,
            Weight = weight,
            Wage = wage,
            ProductType = ProductType.MoltenGold,
            Fineness = fineness,
            GoldUnitType = goldUnitType,
            WageType = wageType,
            WagePriceUnitId = wagePriceUnitId,
            ProductCategoryId = productCategoryId,
            MoltenGold = moltenGold
        };
    }

    public static Product Create(
        string name,
        decimal weight,
        decimal wage,
        ProductType productType,
        decimal fineness,
        GoldUnitType goldUnitType,
        WageType? wageType,
        PriceUnitId? wagePriceUnitId,
        PriceUnitId? stonePriceUnitId,
        ProductCategoryId? productCategoryId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(weight, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(wage, 0);

        if (wageType is Shared.Enums.WageType.Percent && wage > 100)
            throw new ArgumentOutOfRangeException(nameof(wage), $"درصد اجرت برای جنس {name} باید بین 0 الی 100 باشد");

        return new Product
        {
            Id = new ProductId(Guid.CreateVersion7()),
            Name = name,
            Weight = weight,
            Wage = wage,
            ProductType = productType,
            Fineness = fineness,
            GoldUnitType = goldUnitType,
            WageType = wageType,
            WagePriceUnitId = wagePriceUnitId,
            StonePriceUnitId = stonePriceUnitId,
            ProductCategoryId = productCategoryId
        };
    }

#pragma warning disable CS8618
    private Product() { }
#pragma warning restore CS8618

    public string Name { get; private set; }
    public string Barcode { get; private set; }
    public decimal Weight { get; private set; }
    public decimal Wage { get; private set; }
    public ProductType ProductType { get; private set; }
    public decimal Fineness { get; private set; }
    public WageType? WageType { get; private set; }
    public GoldUnitType GoldUnitType { get; private set; }

    public ProductCategoryId? ProductCategoryId { get; private set; }
    public ProductCategory? ProductCategory { get; private set; }

    public PriceUnitId? WagePriceUnitId { get; private set; }
    public PriceUnit? WagePriceUnit { get; private set; }

    public PriceUnitId? StonePriceUnitId { get; private set; }
    public PriceUnit? StonePriceUnit { get; private set; }

    private readonly List<GemStone> _stones = [];
    public IReadOnlyList<GemStone> GemStones => _stones;

    public MoltenGold? MoltenGold { get; private set; }

    public IReadOnlyList<InventoryStock>? InventoryStocks { get; private set; }

    public Product SetName(string name)
    {
        Name = name;
        return this;
    }

    public Product SetBarcode(string barcode)
    {
        Barcode = barcode;
        return this;
    }

    public Product SetWeight(decimal weight)
    {
        Weight = weight;
        return this;
    }

    public Product SetWage(decimal wage)
    {
        Wage = wage;
        return this;
    }

    public Product SetProductType(ProductType productType)
    {
        ProductType = productType;
        return this;
    }

    public Product SetFineness(decimal fineness)
    {
        Fineness = fineness;
        return this;
    }

    public Product SetGoldUnitType(GoldUnitType goldUnitType)
    {
        GoldUnitType = goldUnitType;
        return this;
    }

    public Product SetWageType(WageType? wageType)
    {
        if (wageType is Shared.Enums.WageType.Percent) 
            WagePriceUnitId = null;

        WageType = wageType;
        return this;
    }

    public Product SetProductCategory(ProductCategoryId? categoryId)
    {
        ProductCategoryId = categoryId;
        return this;
    }

    public Product SetGemStones(IEnumerable<GemStone>? stones)
    {
        ClearGemStones();

        if (stones is not null)
            _stones.AddRange(stones);

        return this;
    }
    public void ClearGemStones() => _stones.Clear();

    public Product SetWagePriceUnitId(PriceUnitId? wagePriceUnitId)
    {
        if (WageType is Shared.Enums.WageType.Percent && WagePriceUnitId is not null)
            throw new InvalidOperationException("Percent wage type cannot have wage price unit");

        WagePriceUnitId = wagePriceUnitId;
        return this;
    }

    public void SetMoltenGold(string? assayNumber, DateTime? assayDate, CustomerId? assayerId)
    {
        MoltenGold = MoltenGold.Create(assayNumber, assayDate, assayerId);
    }
}