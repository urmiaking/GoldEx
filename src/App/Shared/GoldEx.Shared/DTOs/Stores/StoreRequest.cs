namespace GoldEx.Shared.DTOs.Stores;

public record StoreRequest(
    string Name,
    string Slug,
    byte[]? LogoContent,
    string? LogoExtension,
    byte[]? BackgroundImageContent,
    string? BackgroundImageExtension,
    bool IsActive
);
