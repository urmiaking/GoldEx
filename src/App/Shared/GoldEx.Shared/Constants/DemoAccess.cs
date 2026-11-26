using System.Security.Claims;

namespace GoldEx.Shared.Constants;

/// <summary>Claims belonging exclusively to GoldEx enterprise demo accounts.</summary>
public static class DemoAccess
{
    public const string Role = "Demo";
    public const string StoreClaim = "goldex:demo-store";
    public const string ExpiryClaim = "goldex:demo-expiry";
    public const string PhoneClaim = "goldex:demo-contact";
    public const string ReadOnlyMessage = "در نسخه آزمایشی، ثبت و تغییر اطلاعات امکان‌پذیر نیست.";
    public const string ExpiredMessage = "مهلت استفاده از نسخه آزمایشی به پایان رسیده است.";

    public static bool IsDemo(ClaimsPrincipal? user) =>
        user?.IsInRole(Role) == true || user?.HasClaim(c => c.Type == ExpiryClaim || c.Type == StoreClaim) == true;

    public static DateTimeOffset? GetExpiry(ClaimsPrincipal? user)
    {
        if (!long.TryParse(user?.FindFirst(ExpiryClaim)?.Value, out var seconds))
            return null;
        try { return DateTimeOffset.FromUnixTimeSeconds(seconds); }
        catch (ArgumentOutOfRangeException) { return null; }
    }
}
