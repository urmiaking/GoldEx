namespace GoldEx.Shared.DTOs.Products;

public record UpdateGemStoneRequest(string Type, string Color, string? Cut, double Carat, string? Purity);