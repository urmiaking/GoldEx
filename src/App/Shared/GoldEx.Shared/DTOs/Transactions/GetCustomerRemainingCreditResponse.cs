using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Transactions;

public record GetCustomerRemainingCreditResponse(double Value, UnitType Unit);