using GoldEx.Shared.DTOs.CoinInstances;
using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class CoinInstanceVm
{
    public Guid? Id { get; set; }

    [Display(Name = "بارکد")]
    public string? Barcode { get; set; }

    [Display(Name = "سکه")]
    public GetCoinResponse? Coin { get; set; }

    [Display(Name = "وزن")]
    [Required(ErrorMessage = "وزن الزامی است")]
    public decimal? Weight { get; set; }

    [Display(Name = "عیار")]
    [Required(ErrorMessage = "عیار الزامی است")]
    public decimal Fineness { get; set; } = 900m;

    [Display(Name = "نوع سکه")]
    public CoinMintType MintType { get; set; } = CoinMintType.Banking;

    [Display(Name = "نوع بسته بندی")]
    public CoinPackageType PackageType { get; set; } = CoinPackageType.VacuumSealed;

    [Display(Name = "سال ضرب")]
    public DateTime? MintYear { get; set; }

    [Display(Name = "بسته بندی سکه")]
    public CoinPackageSpecVm? CoinPackage { get; set; } = new();

    public CoinInstanceRequestDto ToRequest()
    {
        if (Coin is null)
            throw new FluentValidation.ValidationException("سکه انتخاب نشده است");

        return new CoinInstanceRequestDto(
            Id,
            Coin.Id,
            Barcode,
            MintYear?.Year,
            Weight ?? throw new FluentValidation.ValidationException("وزن سکه الزامی است"),
            Fineness,
            MintType,
            PackageType,
            CoinPackage?.ToRequest()
        );
    }

    public static CoinInstanceVm CreateFrom(GetCoinInstanceResponse response)
    {
        return new CoinInstanceVm
        {
            Id = response.Id,
            Barcode = response.Barcode,
            Coin = response.Coin,
            Weight = response.Weight,
            Fineness = response.Fineness,
            MintType = response.MintType,
            PackageType = response.PackageType,
            MintYear = response.MintYear.HasValue ? new DateTime(response.MintYear.Value, 3, 21) : null,
            CoinPackage = response.CoinPackage != null ? CoinPackageSpecVm.CreateFrom(response.CoinPackage) : null
        };
    }
}