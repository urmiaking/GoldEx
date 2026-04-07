namespace GoldEx.Shared.DTOs.UserAccounts;

public record GetTwoFactorAuthStatusResponse(bool Enabled, bool MachineRemembered, int RecoveryCodesLeft);