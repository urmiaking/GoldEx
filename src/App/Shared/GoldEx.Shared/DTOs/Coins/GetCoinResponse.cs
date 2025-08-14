using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Coins;

public record GetCoinResponse(Guid Id, string Title, bool IsActive, Guid? PriceId);