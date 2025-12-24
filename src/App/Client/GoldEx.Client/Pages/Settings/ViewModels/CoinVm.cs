using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.DTOs.Prices;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Settings.ViewModels;

public class CoinVm
{
    public Guid? Id { get; set; }

    [Display(Name = "عنوان")]
    [Required(ErrorMessage = "عنوان الزامی است.")]
    public string? Title { get; set; }

    [Display(Name = "وضعیت")]
    public bool IsActive { get; set; }

    [Display(Name = "وزن")]
    [Required(ErrorMessage = "وزن الزامی است.")]
    public decimal? Weight { get; set; }

    [Display(Name = "عیار")]
    [Required(ErrorMessage = "عیار الزامی است.")]
    public decimal? Fineness { get; set; }

    [Display(Name = "سال شروع ضرب")]
    [Required(ErrorMessage = "سال شروع ضرب الزامی است.")]
    public DateTime? StartMintYear { get; set; }

    [Display(Name = "سال پایان ضرب")]
    public DateTime? EndMintYear { get; set; }

    [Display(Name = "قیمت وابسته")]
    public Guid? PriceId { get; set; }

    public GetPriceTitleResponse? Price { get; set; }

    public static CoinVm CreateFrom(GetCoinResponse response)
    {
        return new CoinVm
        {
            Id = response.Id,
            Title = response.Title,
            Weight = response.Weight,
            Fineness =response.Fineness,
            StartMintYear = response.StartMintYear != 0 ? new DateTime(response.StartMintYear, 1, 1) : null ,
            EndMintYear = response.EndMintYear.HasValue ? new DateTime(response.EndMintYear.Value, 1, 1) : null,
            IsActive = response.IsActive,
            PriceId = response.PriceId
        };
    }

    public static CoinRequestDto ToRequest(CoinVm item) =>
        new(item.Id,
            item.Title ?? string.Empty,
            item.Weight ?? 0,
            item.Fineness ?? 0,
            item.StartMintYear?.Year ?? 0,
            item.EndMintYear?.Year,
            item.PriceId);
}