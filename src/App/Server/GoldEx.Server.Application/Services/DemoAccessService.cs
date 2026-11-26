using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GoldEx.Server.Application.Services;

public sealed record DemoCredentials(string UserName, string Password, DateTimeOffset ExpiresAt);
public sealed record DemoSession(Guid StoreId, string Slug, DateTimeOffset ExpiresAt);
public sealed record DemoAccountStatus(bool IsRestricted, DemoSession? Session);

/// <summary>Demo accounts are real, isolated Identity users. Unverified contact numbers are never login identifiers.</summary>
public sealed class DemoAccessService(
    GoldExDbContext db,
    UserManager<AppUser> users,
    IConfiguration configuration,
    IHttpClientFactory httpClients)
{
    public bool Enabled => configuration.GetValue<bool>("Demo:Enabled");
    public string Host => configuration["Demo:Host"] ?? "demo.goldexsoft.ir";
    public int DurationDays => Math.Clamp(configuration.GetValue("Demo:DurationDays", 7), 1, 30);
    public string? SiteKey => configuration["Turnstile:SiteKey"];

    public bool IsHost(string host) => string.Equals(host, Host, StringComparison.OrdinalIgnoreCase);

    public async Task<Store?> GetStoreAsync(CancellationToken ct = default)
    {
        if (!Enabled || !Guid.TryParse(configuration["Demo:StoreId"], out var id) || id == Guid.Empty)
            return null;
        return await db.Set<Store>().AsNoTracking().SingleOrDefaultAsync(
            s => s.Id == new StoreId(id) && s.Slug == "demo" && s.IsActive, ct);
    }

    public async Task<DemoAccountStatus> GetStatusAsync(AppUser user, string host, CancellationToken ct = default)
    {
        var roles = await users.GetRolesAsync(user);
        var claims = await users.GetClaimsAsync(user);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var restricted = roles.Contains(DemoAccess.Role) || DemoAccess.IsDemo(principal);
        if (!restricted)
            return new(false, null);

        var expiry = DemoAccess.GetExpiry(principal);
        if (!Enabled || !IsHost(host) || roles.Count != 1 || !roles.Contains(DemoAccess.Role)
            || expiry is null || expiry <= DateTimeOffset.UtcNow
            || user.LockoutEnd > DateTimeOffset.UtcNow
            || !Guid.TryParse(principal.FindFirst(DemoAccess.StoreClaim)?.Value, out var storeId))
            return new(true, null);

        var store = await GetStoreAsync(ct);
        if (store is null || store.Id.Value != storeId)
            return new(true, null);

        // Fail closed if somebody accidentally assigns a demo account to a real store.
        var memberships = await db.Set<StoreUser>().AsNoTracking()
            .Where(m => m.UserId == user.Id).Select(m => m.StoreId).ToListAsync(ct);
        if (memberships.Count != 1 || memberships[0] != store.Id)
            return new(true, null);

        return new(true, new DemoSession(storeId, store.Slug, expiry.Value));
    }

    public async Task<DemoSession?> ValidatePrincipalAsync(ClaimsPrincipal principal, string host, CancellationToken ct)
    {
        if (!principal.IsInRole(DemoAccess.Role) || DemoAccess.GetExpiry(principal) is not { } ticketExpiry
            || ticketExpiry <= DateTimeOffset.UtcNow)
            return null;
        var user = await users.GetUserAsync(principal);
        if (user is null)
            return null;
        var status = await GetStatusAsync(user, host, ct);
        if (status.Session is not { } session ||
            principal.FindFirst(DemoAccess.StoreClaim)?.Value != session.StoreId.ToString())
            return null;
        return session;
    }

    public static string? NormalizeMobile(string? input)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length > 32)
            return null;
        var value = string.Concat(input.Trim().Where(c => !char.IsWhiteSpace(c) && c != '-')
            .Select(c => c is >= '۰' and <= '۹' ? (char)('0' + c - '۰')
                : c is >= '٠' and <= '٩' ? (char)('0' + c - '٠') : c));
        if (value.StartsWith("+98", StringComparison.Ordinal))
            value = "0" + value[3..];
        else if (value.StartsWith("0098", StringComparison.Ordinal))
            value = "0" + value[4..];
        return value.Length == 11 && value.StartsWith("09", StringComparison.Ordinal)
            && value.All(c => c is >= '0' and <= '9') ? value : null;
    }

    public async Task<bool> VerifyCaptchaAsync(string? token, string host, CancellationToken ct)
    {
        var secret = configuration["Turnstile:SecretKey"];
        if (!Enabled || !IsHost(host) || string.IsNullOrWhiteSpace(secret)
            || string.IsNullOrWhiteSpace(token) || token.Length > 2048)
            return false;
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["secret"] = secret,
            ["response"] = token
        });
        using var response = await httpClients.CreateClient().PostAsync(
            "https://challenges.cloudflare.com/turnstile/v0/siteverify", form, ct);
        if (!response.IsSuccessStatusCode)
            return false;
        var result = await response.Content.ReadFromJsonAsync<CaptchaResult>(cancellationToken: ct);
        return result?.Success == true && string.Equals(result.Hostname, host, StringComparison.OrdinalIgnoreCase)
            && result.Action == "demo-register";
    }

    public async Task<DemoCredentials?> CreateAsync(string? input, string? captcha, string host, CancellationToken ct)
    {
        var mobile = NormalizeMobile(input);
        if (mobile is null || !await VerifyCaptchaAsync(captcha, host, ct))
            return null;
        var store = await GetStoreAsync(ct);
        if (store is null)
            return null;

        var username = "demo_" + Convert.ToHexString(RandomNumberGenerator.GetBytes(10)).ToLowerInvariant();
        var password = Convert.ToHexString(RandomNumberGenerator.GetBytes(12));
        var expiry = DateTimeOffset.UtcNow.AddDays(DurationDays);
        var user = new AppUser("کاربر آزمایشی", username);
        // Do not populate AppUser.PhoneNumber: the public phone login would then trust an unverified number.
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        Ensure(await users.CreateAsync(user, password));
        Ensure(await users.AddToRoleAsync(user, DemoAccess.Role));
        Ensure(await users.AddClaimsAsync(user, [
            new Claim(DemoAccess.StoreClaim, store.Id.Value.ToString()),
            new Claim(DemoAccess.ExpiryClaim, expiry.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new Claim(DemoAccess.PhoneClaim, mobile)
        ]));
        db.Set<StoreUser>().Add(StoreUser.Create(store.Id, user.Id, isDefault: true));
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return new(username, password, expiry);
    }

    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException("Could not provision demo account.");
    }

    private sealed record CaptchaResult(
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("hostname")] string? Hostname,
        [property: JsonPropertyName("action")] string? Action);
}
