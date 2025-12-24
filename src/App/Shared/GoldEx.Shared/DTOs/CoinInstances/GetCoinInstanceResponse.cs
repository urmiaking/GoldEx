using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.CoinInstances;

public record GetCoinInstanceResponse(Guid Id,
    string Barcode, 
    int? MintYear,
    decimal Weight,
    decimal Fineness,
    CoinMintType MintType,
    CoinPackageType PackageType,
    CoinPackageResponse? CoinPackage,
    GetCoinResponse Coin);

public record CoinPackageResponse(
    decimal VacuumedWeight,
    string StandardCode,
    string? CardColor,
    GetCustomerResponse? Issuer);