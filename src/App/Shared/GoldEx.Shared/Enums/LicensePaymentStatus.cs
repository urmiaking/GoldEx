using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum LicensePaymentStatus
{
    [Display(Name = "در انتظار تایید")]
    Pending = 0,

    [Display(Name = "تایید شده")]
    Approved = 1,

    [Display(Name = "رد شده")]
    Rejected = 2
}