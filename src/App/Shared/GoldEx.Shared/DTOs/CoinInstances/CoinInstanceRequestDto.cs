using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.CoinInstances;

/// <summary>
/// Represents a request to create or update a coin instance.
/// </summary>
/// <param name="Id"></param>
/// <param name="CoinId"></param>
/// <param name="Barcode"></param>
/// <param name="MintYear"></param>
/// <param name="Weight"></param>
/// <param name="Fineness"></param>
/// <param name="MintType"></param>
/// <param name="PackageType"></param>
/// <param name="CoinPackage"></param>
public record CoinInstanceRequestDto(
    Guid? Id,
    Guid CoinId,
    string? Barcode,
    int? MintYear,
    decimal Weight,
    decimal Fineness,
    CoinMintType MintType,
    CoinPackageType PackageType,
    CoinPackageDto? CoinPackage);

public record CoinPackageDto(
    decimal VacuumedWeight,
    string StandardCode,
    string? CardColor,
    Guid? IssuerId);