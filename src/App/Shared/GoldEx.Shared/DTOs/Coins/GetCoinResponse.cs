namespace GoldEx.Shared.DTOs.Coins;

public record GetCoinResponse(
    Guid Id,
    string Title,
    decimal Weight,
    decimal Fineness,
    int StartMintYear,
    int? EndMintYear,
    bool IsActive,
    Guid? PriceId);