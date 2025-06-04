namespace GoldEx.Shared.DTOs.PriceUnits;

public record CreatePriceUnitRequest(string Title, byte[]? IconContent, Guid? PriceId);