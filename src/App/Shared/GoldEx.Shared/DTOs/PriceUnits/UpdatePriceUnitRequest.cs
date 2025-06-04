namespace GoldEx.Shared.DTOs.PriceUnits;

public record UpdatePriceUnitRequest(string Title, byte[]? IconContent, Guid? PriceId);