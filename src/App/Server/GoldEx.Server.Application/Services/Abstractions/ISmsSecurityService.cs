using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface ISmsSecurityService
{
    (bool Allowed, string? ErrorMessage, int? RetryAfterSeconds) CheckCanSendSms(string phoneNumber, string? clientIp);
    Task<string> GenerateAndStoreOtpAsync(string phoneNumber, OtpPurpose purpose, string? clientIp);
    (bool Success, string? ErrorMessage, int RemainingAttempts) VerifyOtp(string phoneNumber, string code, OtpPurpose purpose);
    void InvalidateOtp(string phoneNumber, OtpPurpose purpose);
    bool IsWithinAllowedSmsHours();
}