using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.PriceUnits;

public record GetPriceUnitResponse(Guid Id, string Title, UnitType? UnitType, bool HasIcon, bool IsActive, bool IsDefault, Guid? PriceId, string? PriceTitle);