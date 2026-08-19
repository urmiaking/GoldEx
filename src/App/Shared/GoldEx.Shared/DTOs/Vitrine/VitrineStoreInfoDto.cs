namespace GoldEx.Shared.DTOs.Vitrine;

public record VitrineStoreInfoDto(
    string Name,
    string Slug,
    string? LogoUrl,
    string? BackgroundImageUrl,
    string? Address,
    string? PhoneNumber,
    string? InstagramUrl,
    string? TelegramUrl,
    string? BaleUrl,
    string? WhatsAppNumber,
    string? AboutText,
    decimal LiveGoldPrice18K = 0);
