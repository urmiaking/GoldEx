using System.ComponentModel.DataAnnotations;

namespace GoldEx.Sdk.Common.Definitions;

public enum Gender
{
    [Display(Name = "مرد")]
    Male,

    [Display(Name = "زن")]
    Female,

    [Display(Name = "نامشخص")]
    Unknown = 99,

    [Display(Name = "سایر")]
    Other = 100
}
