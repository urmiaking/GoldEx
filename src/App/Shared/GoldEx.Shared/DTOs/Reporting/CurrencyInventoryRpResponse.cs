using GoldEx.Shared.DTOs.PriceUnits;

namespace GoldEx.Shared.DTOs.Reporting;

public record CurrencyInventoryRpResponse(DateTime DateTime,
    GetPriceUnitTitleResponse Currency,
    decimal CurrentAmount,
    decimal SoldAmount);