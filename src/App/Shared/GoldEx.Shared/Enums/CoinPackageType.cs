using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum CoinPackageType
{
    [Display(Name = "وکیوم شده")]
    VacuumSealed = 0,

    [Display(Name = "باز")]
    Open = 1,
}