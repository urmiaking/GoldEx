namespace GoldEx.Shared.DTOs.UserAccounts;

public record EnableTwoFactorAuthResponse(IEnumerable<string> RecoveryCodes);