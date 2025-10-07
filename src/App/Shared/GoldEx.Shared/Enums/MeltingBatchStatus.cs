using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum MeltingBatchStatus
{
    [Display(Name = "در حال ذوب")]     // ذوب فیزیکی انجام شده، منتظر نمونه‌گیری
    Melting = 1,

    [Display(Name = "ارسال به آزمایشگاه")] // نمونه فرستاده شده
    SentToLab = 2,

    [Display(Name = "تکمیل شده")]       // ری‌گیری انجام شده، آبشده وارد انبار
    Completed = 3
}