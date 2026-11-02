using System.Security.Cryptography;
using System.Text;
using GoldEx.Shared.DTOs.Notifications;
using FluentValidation.Results;

namespace GoldEx.Server.Application.Services;

public static class PushSubscriptionValidator
{
    public static string Hash(string endpoint) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(endpoint)));
    public static bool ValidEndpoint(string? value) => value is { Length: <= 2048 } &&
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == "https" && uri.IsDefaultPort &&
        string.IsNullOrEmpty(uri.UserInfo) && string.IsNullOrEmpty(uri.Fragment) &&
        (uri.Host.Equals("fcm.googleapis.com", StringComparison.OrdinalIgnoreCase) ||
         // Chromium also issues subscriptions through its legacy staging GCM endpoint.
         (uri.Host.Equals("jmt17.google.com", StringComparison.OrdinalIgnoreCase) &&
          uri.AbsolutePath.StartsWith("/fcm/send/", StringComparison.Ordinal)) ||
         uri.Host.Equals("updates.push.services.mozilla.com", StringComparison.OrdinalIgnoreCase) ||
         uri.Host.Equals("push.services.mozilla.com", StringComparison.OrdinalIgnoreCase) ||
         uri.Host.EndsWith(".push.apple.com", StringComparison.OrdinalIgnoreCase) ||
         uri.Host.Equals("web.push.apple.com", StringComparison.OrdinalIgnoreCase) ||
         uri.Host.EndsWith(".notify.windows.com", StringComparison.OrdinalIgnoreCase));

    public static bool Valid(RegisterPushRequest request) => GetErrors(request).Count == 0;

    public static IReadOnlyList<ValidationFailure> GetErrors(RegisterPushRequest request)
    {
        static ValidationFailure Error(string property, string code, string message) => new(property, message) { ErrorCode = code };
        if (!ValidEndpoint(request.Endpoint))
            return [Error(nameof(request.Endpoint), "Endpoint.Unsupported", "نشانی سرویس اعلان این مرورگر پشتیبانی نمی‌شود.")];
        if (string.IsNullOrWhiteSpace(request.P256dh) || request.P256dh.Length > 128)
            return [Error(nameof(request.P256dh), "P256dh.MissingOrOversized", "اطلاعات امنیتی دستگاه کامل نیست. اعلان دستگاه را دوباره فعال کنید.")];
        if (string.IsNullOrWhiteSpace(request.Auth) || request.Auth.Length > 64)
            return [Error(nameof(request.Auth), "Auth.MissingOrOversized", "اطلاعات امنیتی دستگاه کامل نیست. اعلان دستگاه را دوباره فعال کنید.")];
        try
        {
            var key = Decode(request.P256dh);
            var auth = Decode(request.Auth);
            if (key.Length != 65 || key[0] != 4)
                return [Error(nameof(request.P256dh), "P256dh.LengthOrFormat", "اطلاعات امنیتی این دستگاه معتبر نیست. اعلان دستگاه را دوباره فعال کنید.")];
            if (auth.Length != 16)
                return [Error(nameof(request.Auth), "Auth.Length", "اطلاعات امنیتی این دستگاه معتبر نیست. اعلان دستگاه را دوباره فعال کنید.")];
            using var curve = ECDiffieHellman.Create(new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                Q = new ECPoint { X = key[1..33], Y = key[33..65] }
            });
            return [];
        }
        catch (Exception ex) when (ex is FormatException or CryptographicException or ArgumentException)
        {
            return [Error(nameof(request.P256dh), "Keys.InvalidEncodingOrPoint", "اطلاعات امنیتی این دستگاه معتبر نیست. اعلان دستگاه را دوباره فعال کنید.")];
        }
    }
    private static byte[] Decode(string value) => Convert.FromBase64String(value.Replace('-', '+').Replace('_', '/').PadRight((value.Length + 3) / 4 * 4, '='));
}
