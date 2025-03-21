namespace GoldEx.Shared.DTOs.Products;

public record GetGemStoneResponse(Guid Id, string Type, string Color, string? Cut, double Carat, string? Purity);