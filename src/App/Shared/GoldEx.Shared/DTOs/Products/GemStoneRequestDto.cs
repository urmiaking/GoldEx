namespace GoldEx.Shared.DTOs.Products;

public record GemStoneRequestDto(string? Code, string Type, string Color, string? Cut, decimal Carat, decimal Cost, string? Purity);