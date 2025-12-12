namespace GoldEx.Shared.DTOs.Coins;

public record GetCoinResponse(Guid Id, string Title, decimal Weight, decimal Fineness, bool IsActive, Guid? PriceId);