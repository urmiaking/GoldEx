using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.LedgerAccounts;

public record LedgerAccountRequestDto(Guid? Id, string Title, bool IsSystemAccount, LedgerAccountType AccountType, Guid? CustomerId, Guid? ParentAccountId);