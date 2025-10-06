using GoldEx.Shared.DTOs.PriceUnits;

namespace GoldEx.Shared.DTOs.Transactions;

public record GetCustomerRemainingResponse(GetPriceUnitTitleResponse PriceUnit, decimal Amount);