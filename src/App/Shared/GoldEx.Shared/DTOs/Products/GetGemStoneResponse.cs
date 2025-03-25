namespace GoldEx.Shared.DTOs.Products;

public record GetGemStoneResponse(string Code, string Type, string Color, string? Cut, double Carat, string? Purity);