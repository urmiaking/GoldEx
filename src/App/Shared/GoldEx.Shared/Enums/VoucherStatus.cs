using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum VoucherStatus
{
    [Display(Name = "در انتظار اعمال")]
    Pending,

    [Display(Name = "اعمال شده")]
    Applied
}