using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Coins;

public record CoinRequestDto(Guid? Id, string Title, CoinType CoinType, Guid? PriceId);