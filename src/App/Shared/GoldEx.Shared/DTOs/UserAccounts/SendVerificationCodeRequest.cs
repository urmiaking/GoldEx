namespace GoldEx.Shared.DTOs.UserAccounts;

public record SendVerificationCodeRequest(string OldPhoneNumber, string NewPhoneNumber);