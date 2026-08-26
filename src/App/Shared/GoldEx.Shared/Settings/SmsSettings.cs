namespace GoldEx.Shared.Settings;

public class SmsSettings
{
    public string SenderPhoneNumber { get; set; } = default!;
    public string ApiKey { get; set; } = default!;

    /// <summary>
    /// Minimum seconds between SMS sends to the same phone number (default: 120s).
    /// </summary>
    public int CooldownSeconds { get; set; } = 120;

    /// <summary>
    /// Maximum SMS allowed per phone number per hour (default: 5).
    /// </summary>
    public int MaxPerHourPerPhone { get; set; } = 5;

    /// <summary>
    /// Maximum SMS allowed per phone number per day (default: 10).
    /// </summary>
    public int MaxPerDayPerPhone { get; set; } = 10;

    /// <summary>
    /// Maximum SMS requests allowed from a single IP address per 10 minutes (default: 5).
    /// </summary>
    public int MaxPer10MinPerIp { get; set; } = 5;

    /// <summary>
    /// Global system-wide SMS limit per hour (default: 100).
    /// </summary>
    public int GlobalHourlyLimit { get; set; } = 100;

    /// <summary>
    /// OTP expiration in minutes (default: 3 minutes).
    /// </summary>
    public int OtpExpirationMinutes { get; set; } = 3;

    /// <summary>
    /// Maximum failed OTP verification attempts before code is invalidated (default: 5).
    /// </summary>
    public int MaxFailedVerificationAttempts { get; set; } = 5;
}