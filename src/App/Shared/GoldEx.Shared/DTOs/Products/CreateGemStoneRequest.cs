namespace GoldEx.Shared.DTOs.Products;

public record CreateGemStoneRequest(Guid Id, string Type, string Color, string? Cut, double Carat, string? Purity);