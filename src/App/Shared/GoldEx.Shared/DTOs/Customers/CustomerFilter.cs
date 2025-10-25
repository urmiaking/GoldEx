using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Customers;

public record CustomerFilter(CustomerType? CustomerType, DateTime? Start, DateTime? End);