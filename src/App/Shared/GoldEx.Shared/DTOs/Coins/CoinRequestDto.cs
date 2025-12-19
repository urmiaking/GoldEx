namespace GoldEx.Shared.DTOs.Coins;

public record CoinRequestDto(Guid? Id, string Title, decimal Weight, decimal Fineness, int StartMintYear, int? EndMintYear, Guid? PriceId);