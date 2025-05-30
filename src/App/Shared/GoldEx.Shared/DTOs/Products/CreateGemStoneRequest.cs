namespace GoldEx.Shared.DTOs.Products;

public record CreateGemStoneRequest(string Code, string Type, string Color, string? Cut, decimal Carat, string? Purity);