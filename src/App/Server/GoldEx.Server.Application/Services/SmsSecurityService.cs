using System.Security.Cryptography;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GoldEx.Server.Application.Services;

[ScopedService]
public class SmsSecurityService(
    IMemoryCache cache,
    IOptions<SmsSettings> options,
    ILogger<SmsSecurityService> logger) : ISmsSecurityService
{
    private readonly SmsSettings _settings = options.Value;

    public bool IsWithinAllowedSmsHours()
    {
        var now = DateTime.Now;
        var start = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);
        var end = start.AddHours(14); // 22:00

        return now > start && now <= end;
    }

    public (bool Allowed, string? ErrorMessage, int? RetryAfterSeconds) CheckCanSendSms(string phoneNumber, string? clientIp)
    {
        var normalizedPhone = NormalizePhoneNumber(phoneNumber);

        // 1. Allowed Hours Check (08:00 to 22:00)
#if !DEBUG
        if (!IsWithinAllowedSmsHours())
        {
            return (false, "ارسال پیامک در ساعات ۲۲:۰۰ الی ۰۸:۰۰ امکان‌پذیر نمی‌باشد. لطفاً از طریق نام کاربری و کلمه عبور وارد شوید.", null);
        }
#endif

        // 2. Cooldown check per phone number
        var cooldownKey = $"sms:cooldown:{normalizedPhone}";
        if (cache.TryGetValue(cooldownKey, out DateTime cooldownExpiresAt))
        {
            var remainingSeconds = (int)Math.Ceiling((cooldownExpiresAt - DateTime.UtcNow).TotalSeconds);
            if (remainingSeconds > 0)
            {
                return (false, $"لطفاً {remainingSeconds} ثانیه دیگر مجدداً تلاش کنید.", remainingSeconds);
            }
        }

        // 3. Hourly limit per phone number
        var currentHour = DateTime.UtcNow.ToString("yyyyMMddHH");
        var phoneHourKey = $"sms:count:phone:hr:{normalizedPhone}:{currentHour}";
        var phoneHourCount = cache.Get<int?>(phoneHourKey) ?? 0;
        if (phoneHourCount >= _settings.MaxPerHourPerPhone)
        {
            logger.LogWarning("SMS rate limit exceeded (hourly) for phone: {Phone}", normalizedPhone);
            return (false, "تعداد درخواست‌های پیامک برای این شماره بیش از حد مجاز است. لطفاً ساعتی دیگر تلاش کنید.", 3600);
        }

        // 4. Daily limit per phone number
        var currentDay = DateTime.UtcNow.ToString("yyyyMMdd");
        var phoneDayKey = $"sms:count:phone:day:{normalizedPhone}:{currentDay}";
        var phoneDayCount = cache.Get<int?>(phoneDayKey) ?? 0;
        if (phoneDayCount >= _settings.MaxPerDayPerPhone)
        {
            logger.LogWarning("SMS rate limit exceeded (daily) for phone: {Phone}", normalizedPhone);
            return (false, "سقف روزانه درخواست پیامک برای این شماره تکمیل شده است. لطفاً فردا تلاش نمایید.", 86400);
        }

        // 5. IP-based rate limit
        if (!string.IsNullOrEmpty(clientIp))
        {
            var tenMinuteWindow = (DateTime.UtcNow.Ticks / TimeSpan.FromMinutes(10).Ticks).ToString();
            var ipKey = $"sms:count:ip:{clientIp}:{tenMinuteWindow}";
            var ipCount = cache.Get<int?>(ipKey) ?? 0;
            if (ipCount >= _settings.MaxPer10MinPerIp)
            {
                logger.LogWarning("SMS rate limit exceeded for IP: {IP}", clientIp);
                return (false, "تعداد درخواست‌های ارسالی از این آدرس بیش از حد مجاز است. لطفاً دقایقی دیگر تلاش کنید.", 600);
            }
        }

        // 6. Global hourly circuit breaker
        var globalHourKey = $"sms:count:global:hr:{currentHour}";
        var globalCount = cache.Get<int?>(globalHourKey) ?? 0;
        if (globalCount >= _settings.GlobalHourlyLimit)
        {
            logger.LogError("Global SMS limit reached ({Count}/{Max})", globalCount, _settings.GlobalHourlyLimit);
            return (false, "سیستم ارسال پیامک موقتاً با محدودیت مواجه شده است. لطفاً دقایقی دیگر تلاش فرمایید.", 1800);
        }

        return (true, null, null);
    }

    public Task<string> GenerateAndStoreOtpAsync(string phoneNumber, OtpPurpose purpose, string? clientIp)
    {
        var normalizedPhone = NormalizePhoneNumber(phoneNumber);
        var currentHour = DateTime.UtcNow.ToString("yyyyMMddHH");
        var currentDay = DateTime.UtcNow.ToString("yyyyMMdd");

        // 1. Generate cryptographically secure 6-digit OTP
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");

        // 2. Set cooldown
        var cooldownSeconds = Math.Max(30, _settings.CooldownSeconds);
        var cooldownExpiresAt = DateTime.UtcNow.AddSeconds(cooldownSeconds);
        cache.Set($"sms:cooldown:{normalizedPhone}", cooldownExpiresAt, TimeSpan.FromSeconds(cooldownSeconds));

        // 3. Increment counters
        var phoneHourKey = $"sms:count:phone:hr:{normalizedPhone}:{currentHour}";
        var phoneHourCount = (cache.Get<int?>(phoneHourKey) ?? 0) + 1;
        cache.Set(phoneHourKey, phoneHourCount, TimeSpan.FromHours(1));

        var phoneDayKey = $"sms:count:phone:day:{normalizedPhone}:{currentDay}";
        var phoneDayCount = (cache.Get<int?>(phoneDayKey) ?? 0) + 1;
        cache.Set(phoneDayKey, phoneDayCount, TimeSpan.FromDays(1));

        if (!string.IsNullOrEmpty(clientIp))
        {
            var tenMinuteWindow = (DateTime.UtcNow.Ticks / TimeSpan.FromMinutes(10).Ticks).ToString();
            var ipKey = $"sms:count:ip:{clientIp}:{tenMinuteWindow}";
            var ipCount = (cache.Get<int?>(ipKey) ?? 0) + 1;
            cache.Set(ipKey, ipCount, TimeSpan.FromMinutes(10));
        }

        var globalHourKey = $"sms:count:global:hr:{currentHour}";
        var globalCount = (cache.Get<int?>(globalHourKey) ?? 0) + 1;
        cache.Set(globalHourKey, globalCount, TimeSpan.FromHours(1));

        // 4. Store OTP in cache
        var otpKey = GetOtpCacheKey(purpose, normalizedPhone);
        var otpEntry = new OtpCacheEntry(
            Code: code,
            ExpiresAt: DateTime.UtcNow.AddMinutes(_settings.OtpExpirationMinutes),
            FailedAttempts: 0,
            MaxAttempts: _settings.MaxFailedVerificationAttempts
        );

        cache.Set(otpKey, otpEntry, TimeSpan.FromMinutes(_settings.OtpExpirationMinutes));

        logger.LogInformation("Generated OTP for phone {Phone}, purpose {Purpose}", normalizedPhone, purpose);
        return Task.FromResult(code);
    }

    public (bool Success, string? ErrorMessage, int RemainingAttempts) VerifyOtp(string phoneNumber, string code, OtpPurpose purpose)
    {
        var normalizedPhone = NormalizePhoneNumber(phoneNumber);
        var otpKey = GetOtpCacheKey(purpose, normalizedPhone);

        if (!cache.TryGetValue(otpKey, out OtpCacheEntry? otpEntry) || otpEntry is null)
        {
            return (false, "کد تایید منقضی شده است یا وجود ندارد.", 0);
        }

        if (DateTime.UtcNow > otpEntry.ExpiresAt)
        {
            cache.Remove(otpKey);
            return (false, "اعتبار کد تایید به پایان رسیده است. لطفاً کد جدید دریافت کنید.", 0);
        }

        if (!string.Equals(otpEntry.Code.Trim(), code?.Trim(), StringComparison.Ordinal))
        {
            var newFailedAttempts = otpEntry.FailedAttempts + 1;
            var remaining = Math.Max(0, otpEntry.MaxAttempts - newFailedAttempts);

            if (newFailedAttempts >= otpEntry.MaxAttempts)
            {
                cache.Remove(otpKey);
                logger.LogWarning("OTP invalidated due to max failed attempts for phone {Phone}", normalizedPhone);
                return (false, "تعداد دفعات ورود اشتباه بیش از حد مجاز بود. کد باطل گردید؛ لطفاً مجدداً درخواست دهید.", 0);
            }

            var updatedEntry = otpEntry with { FailedAttempts = newFailedAttempts };
            var remainingTime = otpEntry.ExpiresAt - DateTime.UtcNow;
            if (remainingTime > TimeSpan.Zero)
            {
                cache.Set(otpKey, updatedEntry, remainingTime);
            }

            return (false, $"کد تایید وارد شده نادرست است. ({remaining} تلاش باقی‌مانده)", remaining);
        }

        // OTP is valid -> Remove immediately (Single-Use Guarantee)
        cache.Remove(otpKey);
        logger.LogInformation("OTP successfully verified for phone {Phone}, purpose {Purpose}", normalizedPhone, purpose);
        return (true, null, otpEntry.MaxAttempts);
    }

    public void InvalidateOtp(string phoneNumber, OtpPurpose purpose)
    {
        var normalizedPhone = NormalizePhoneNumber(phoneNumber);
        var otpKey = GetOtpCacheKey(purpose, normalizedPhone);
        cache.Remove(otpKey);
    }

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty;

        var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
        if (cleaned.StartsWith("98") && cleaned.Length == 12)
        {
            cleaned = "0" + cleaned[2..];
        }
        return cleaned;
    }

    private static string GetOtpCacheKey(OtpPurpose purpose, string phoneNumber)
        => $"otp:{purpose}:{phoneNumber}";

    private sealed record OtpCacheEntry(
        string Code,
        DateTime ExpiresAt,
        int FailedAttempts,
        int MaxAttempts);
}