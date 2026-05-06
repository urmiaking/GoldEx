using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Customers;

public record CustomerFilter(CustomerType? CustomerType, TransactionType? TransactionType, DateTime? Start, DateTime? End);