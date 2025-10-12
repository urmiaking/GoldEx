using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.MeltingBatches;

public record GetMeltingBatchTransactionSummary(string Description, decimal Amount, TransactionType TransactionType, string LedgerAccount, DateTime CreatedAt, string PriceUnit);