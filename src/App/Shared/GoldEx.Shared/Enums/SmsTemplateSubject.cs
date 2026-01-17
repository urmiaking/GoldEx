using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum SmsTemplateSubject
{
    [Display(Name = "سررسید فاکتور")]
    DueInvoice = 1
}