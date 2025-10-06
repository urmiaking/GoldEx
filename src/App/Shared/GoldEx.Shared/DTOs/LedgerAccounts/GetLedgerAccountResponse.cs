using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.LedgerAccounts;

public record GetLedgerAccountResponse(Guid Id, string Title, LedgerAccountType AccountType, bool IsSystemAccount, GetLedgerAccountResponse? ParentAccount);