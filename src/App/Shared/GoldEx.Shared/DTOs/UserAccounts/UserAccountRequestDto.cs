namespace GoldEx.Shared.DTOs.UserAccounts;

public record UserAccountRequestDto(
    Guid? Id,
    string FullName,
    string Username,
    string? Password,
    string? PhoneNumber,
    string? Email,
    string Role);