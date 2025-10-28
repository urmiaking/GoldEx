using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Transactions;

public record GetTransactionResponse(
    Guid GroupId,
    string Description,
    decimal Amount,
    string PriceUnit,
    TransactionType TransactionType,
    string LedgerAccount,
    DateTime PostingDate);