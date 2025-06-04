namespace GoldEx.Shared.DTOs.PriceUnits;

public record GetPriceUnitResponse(Guid Id, string Title, bool HasIcon, bool IsActive, Guid? PriceId, string? PriceTitle);