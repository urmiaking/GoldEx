using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Reporting;

public record LedgerAccountTrialBalanceRpResponse(
    Guid Id,
    string LedgerAccountTitle, 
    LedgerAccountType LedgerAccountType,
    string BasePriceUnitTitle,
    decimal DebitAmountBase,
    decimal CreditAmountBase,
    Guid? ParentAccountId,
    List<LedgerAccountTrialBalanceRpResponse> SubLedgerAccounts);