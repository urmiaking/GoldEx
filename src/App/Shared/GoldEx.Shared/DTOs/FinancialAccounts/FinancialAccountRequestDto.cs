using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.FinancialAccounts;

public record FinancialAccountRequestDto(Guid? Id,
    FinancialAccountType FinancialAccountType,
    Guid PriceUnitId,
    LocalBankAccountRequestDto? LocalBankAccount,
    InternationalBankAccountRequestDto? InternationalBankAccount);