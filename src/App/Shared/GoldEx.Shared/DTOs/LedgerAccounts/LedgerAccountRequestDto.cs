using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.LedgerAccounts;

public record LedgerAccountRequestDto(Guid? Id, string Title, LedgerAccountType AccountType, Guid? ParentAccountId);