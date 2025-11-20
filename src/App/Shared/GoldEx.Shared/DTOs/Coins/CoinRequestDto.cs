namespace GoldEx.Shared.DTOs.Coins;

public record CoinRequestDto(Guid? Id, string Title, Guid? PriceId);