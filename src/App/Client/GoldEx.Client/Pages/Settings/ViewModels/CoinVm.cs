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

    [Display(Name = "قیمت وابسته")]
    public Guid? PriceId { get; set; }

    public GetPriceTitleResponse? Price { get; set; }

    public static CoinVm CreateFrom(GetCoinResponse response)
    {
        return new CoinVm
        {
            Id = response.Id,
            Title = response.Title,
            IsActive = response.IsActive,
            PriceId = response.PriceId
        };
    }

    public static CoinRequestDto ToRequest(CoinVm item) => new(item.Id, item.Title ?? string.Empty, item.PriceId);
}