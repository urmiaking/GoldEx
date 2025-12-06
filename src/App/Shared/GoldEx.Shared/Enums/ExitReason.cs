using GoldEx.Shared.Constants;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum ExitReason
{
    [Display(Name = "کسری انبار")]
    Shortage = 1,  

    [Display(Name = "سرقت")]
    Theft = 2,    

    [Display(Name = "فقدان بارکد/شناسه")]
    BarcodeMissing = 3,      

    [Display(Name = "آسیب‌دیدگی/ضایعات")]
    Damage = 4,              

    [Display(Name = "پاداش")]
    Gift = 6,          

    [Display(Name = "سایر")]
    Other = 99
}

public static class ExitReasonExtensions
{
    public static string GetLedgerAccount(this ExitReason reason) => reason switch
    {
        ExitReason.Shortage => SystemLedgerAccounts.ShortageExpense,
        ExitReason.Theft => SystemLedgerAccounts.TheftLoss,
        ExitReason.BarcodeMissing => SystemLedgerAccounts.ShortageExpense,
        ExitReason.Damage => SystemLedgerAccounts.DamageExpense,
        ExitReason.Gift => SystemLedgerAccounts.GiftExpense,
        _ => SystemLedgerAccounts.OperatingExpenses
    };
}