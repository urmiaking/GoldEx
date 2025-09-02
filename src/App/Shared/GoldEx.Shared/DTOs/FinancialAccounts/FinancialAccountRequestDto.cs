using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.FinancialAccounts;

public record FinancialAccountRequestDto(
    Guid? Id,
    FinancialAccountType FinancialAccountType,
    string? HolderName, 
    string? BrokerName,
    Guid PriceUnitId,
    Guid? CustomerId,
    bool IsSystemAccount,
    LocalBankAccountRequestDto? LocalBankAccount,
    InternationalBankAccountRequestDto? InternationalBankAccount,
    CashAccountRequestDto? CashAccount);