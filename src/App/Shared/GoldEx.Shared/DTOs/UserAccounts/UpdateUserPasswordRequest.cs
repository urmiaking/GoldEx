namespace GoldEx.Shared.DTOs.UserAccounts;

public record UpdateUserPasswordRequest(string OldPassword, string NewPassword);