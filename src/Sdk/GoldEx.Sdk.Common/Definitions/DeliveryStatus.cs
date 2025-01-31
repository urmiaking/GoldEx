using System.ComponentModel.DataAnnotations;

namespace GoldEx.Sdk.Common.Definitions;

public enum DeliveryStatus
{
    [Display(Name = "ارسال شده")]
    Sent,

    [Display(Name = "عدم ارسال")]
    NotSent,

    [Display(Name = "رسیده")]
    Delivered,

    [Display(Name = "منقضی")]
    Expired,

    [Display(Name = "مسدود")]
    Blocked,

    [Display(Name = "نامشخص")]
    Unknown
}
