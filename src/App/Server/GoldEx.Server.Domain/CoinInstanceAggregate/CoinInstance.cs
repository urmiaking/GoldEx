using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.CoinInstanceAggregate;

public readonly record struct CoinInstanceId(Guid Value);

/// <summary>
/// Represents a uniquely identifiable batch of homogeneous coins
/// that are packaged and moved together under a single barcode.
/// </summary>
public class CoinInstance : EntityBase<CoinInstanceId>
{
    public string Barcode { get; private set; }
    public int? MintYear { get; private set; }
    public decimal Weight { get; private set; }
    public decimal Fineness { get; private set; }

    public CoinMintType MintType { get; private set; }
    public CoinPackageType PackageType { get; private set; }

    public CoinId CoinId { get; private set; }
    public Coin? Coin { get; private set; }

    public IReadOnlyList<InventoryStock>? InventoryStocks { get; private set; }

    public CoinInstancePackage? CoinInstancePackage { get; private set; }

    private CoinInstance(string barcode,
        int? mintYear,
        decimal weight,
        decimal fineness,
        CoinId coinId,
        CoinMintType mintType,
        CoinPackageType packageType,
        CoinInstancePackage? coinInstancePackage)
        : base(new CoinInstanceId(Guid.CreateVersion7()))
    {
        if (string.IsNullOrWhiteSpace(barcode))
            throw new InvalidOperationException("Barcode is required");

        if (packageType == CoinPackageType.Open && coinInstancePackage != null)
            throw new InvalidOperationException("Open coins cannot have package details");

        if (packageType == CoinPackageType.VacuumSealed && coinInstancePackage == null)
            throw new InvalidOperationException("Vacuumed coins must have package details");

        if (weight <= 0)
            throw new InvalidOperationException("Weight must be greater than zero");

        if (fineness <= 0)
            throw new InvalidOperationException("Fineness must be greater than zero");

        Barcode = barcode;
        MintYear = mintYear;
        Weight = weight;
        Fineness = fineness;
        CoinId = coinId;
        MintType = mintType;
        PackageType = packageType;
        CoinInstancePackage = coinInstancePackage;
    }

#pragma warning disable CS8618
    private CoinInstance() { }
#pragma warning restore CS8618

    public static CoinInstance CreateOpen(string barcode,
        int? mintYear,
        decimal weight,
        decimal fineness,
        CoinId coinId,
        CoinMintType mintType)
    {
        return new CoinInstance(barcode, mintYear, weight, fineness, coinId, mintType, CoinPackageType.Open, null);
    }

    public static CoinInstance CreateVacuumed(string barcode,
        int? mintYear,
        decimal weight,
        decimal fineness,
        CoinId coinId,
        CoinMintType mintType,
        CoinInstancePackage coinInstancePackage)
    {
        return new CoinInstance(barcode, mintYear, weight, fineness, coinId, mintType, CoinPackageType.VacuumSealed, coinInstancePackage);
    }

    public void SetBarcode(string barcode) => Barcode = barcode;
    public void SetMintYear(int? mintYear) => MintYear = mintYear;
    public void SetWeight(decimal weight)
    {
        if (weight <= 0)
            throw new InvalidOperationException("Weight must be greater than zero");
        Weight = weight;
    }
    public void SetFineness(decimal fineness)
    {
        if (fineness <= 0)
            throw new InvalidOperationException("Fineness must be greater than zero");
        Fineness = fineness;
    }
    public void SetMintType(CoinMintType mintType) => MintType = mintType;

    public void SetPackage(CoinPackageType packageType, CoinInstancePackage? package)
    {
        if (packageType == CoinPackageType.Open && package != null)
            throw new InvalidOperationException("Open coins cannot have package details");

        if (packageType == CoinPackageType.VacuumSealed && package == null)
            throw new InvalidOperationException("Vacuumed coins must have package details");

        PackageType = packageType;
        CoinInstancePackage = package;
    }
}