using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum CoinMintType
{
    [Display(Name = "سکه بانکی")]
    Banking = 0,

    [Display(Name = "سکه غیربانکی")]
    NonBanking = 1
}