using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.FinancialAccounts;

public record FinancialAccountRequestDto(
    Guid? Id,
    FinancialAccountType FinancialAccountType,
    Guid PriceUnitId,
    Guid? CustomerId,
    Guid? LedgerAccountId,
    bool IsSystemAccount,
    LocalBankAccountRequestDto? LocalBankAccount,
    InternationalBankAccountRequestDto? InternationalBankAccount);