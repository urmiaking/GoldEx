using GoldEx.Shared.Constants;
using GoldEx.Sdk.Common;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace GoldEx.Server.Components.Account.Pages;

public partial class Login
{
    private string? _errorMessage;
    private string _siteKey = string.Empty;
    private bool _showCaptcha;
    private GoldEx.Server.Application.Services.DemoCaptcha? _demoChallenge;

    private void RefreshDemoCaptcha()
    {
        if (!IsDemoHost) return;
        HttpContext.Session.SetString("DemoCaptchaSession", "1");
        _demoChallenge = Captcha.Issue(HttpContext.Session.Id, "login");
    }

    [CascadingParameter] private HttpContext HttpContext { get; set; } = default!;
    [SupplyParameterFromForm] private InputModel Input { get; set; } = new();
    [SupplyParameterFromQuery] private string? ReturnUrl { get; set; }
    [SupplyParameterFromQuery(Name = "demoExpired")] private string? DemoExpired { get; set; }
    private bool IsDemoHost => Demo.Enabled && Demo.IsHost(HttpContext.Request.Host.Host);

    protected override async Task OnInitializedAsync()
    {
        if (DemoExpired == "1")
            _errorMessage = DemoAccess.ExpiredMessage;
        _siteKey = IsDemoHost ? "" : Config["Turnstile:SiteKey"] ?? throw new InvalidOperationException("Turnstile:SiteKey is missing");

        // Check failed attempts from session
        var failedAttempts = HttpContext.Session.GetInt32("FailedLoginCount") ?? 0;
        _showCaptcha = failedAttempts > (IsDemoHost ? 2 : 5);
        if (_showCaptcha) RefreshDemoCaptcha();

        if (HttpMethods.IsGet(HttpContext.Request.Method))
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }
    }

    private async Task LoginUser()
    {
        var failedAttempts = HttpContext.Session.GetInt32("FailedLoginCount") ?? 0;

        //If failedAttempts > 5, require CAPTCHA
        if (_showCaptcha && IsDemoHost)
        {
            if (!Captcha.Verify(Input.CaptchaId, Input.CaptchaAnswer, HttpContext.Session.Id, "login"))
            {
                Input.CaptchaAnswer = "";
                _errorMessage = "کد امنیتی اشتباه است یا منقضی شده است.";
                return;
            }
        }
        else if (_showCaptcha)
        {
            var captchaResponse = HttpContext.Request.Form["cf-turnstile-response"].ToString();
            if (string.IsNullOrWhiteSpace(captchaResponse))
            {
                _errorMessage = "لطفاً captcha را تکمیل کنید.";
                return;
            }

            var captchaValid = await VerifyCaptchaAsync(captchaResponse);
            if (!captchaValid)
            {
                _errorMessage = "اعتبارسنجی captcha ناموفق بود.";
                return;
            }
        }

        if (!string.IsNullOrEmpty(Input.Passkey?.Error))
            return;

        if (!string.IsNullOrEmpty(Input.Passkey?.CredentialJson))
        {
            Console.WriteLine(@"Performing passkey sign-in...");

            // When performing passkey sign-in, don't perform form validation.
            var passkeySignInResult = await SignInManager.PasskeySignInAsync(Input.Passkey.CredentialJson);

            if (passkeySignInResult.Succeeded)
            {
                RedirectManager.RedirectTo(ReturnUrl);
            }
            else if (passkeySignInResult.RequiresTwoFactor)
            {
                RedirectManager.RedirectTo(
                    "Account/LoginWith2fa",
                    new() { ["returnUrl"] = ReturnUrl });
                return;
            }
            else if (passkeySignInResult.IsLockedOut)
            {
                RedirectManager.RedirectTo("Account/Lockout");
                return;
            }
            else
            {
                RedirectManager.RedirectToCurrentPageWithStatus("خطایی در احراز هویت رخ داد. لطفا مجدد تلاش کنید", HttpContext);
                return;
            }
        }

        Console.WriteLine(@"Performing password sign-in...");

        var result = await SignInManager.PasswordSignInAsync(Input.Username, Input.Password, Input.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var user = await SignInManager.UserManager.FindByNameAsync(Input.Username);
            if (user is null)
            {
                failedAttempts++;
                HttpContext.Session.SetInt32("FailedLoginCount", failedAttempts);
                _errorMessage = "خطا: اطلاعات ورودی معتبر نیست";
                await SignInManager.SignOutAsync();
                return;
            }

            var roles = await SignInManager.UserManager.GetRolesAsync(user);
            var allowedRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { BuiltinRoles.Owners, BuiltinRoles.Administrators };
            var onlyAllowedRoles = (roles.Count == 1 && roles.Contains(DemoAccess.Role))
                || (roles.Count > 0 && roles.All(r => allowedRoles.Contains(r)));
            if (!onlyAllowedRoles)
            {
                failedAttempts++;
                HttpContext.Session.SetInt32("FailedLoginCount", failedAttempts);
                _errorMessage = "خطا: اطلاعات ورودی معتبر نیست";
                await SignInManager.SignOutAsync();
                return;
            }

            // reset failed attempts
            HttpContext.Session.SetInt32("FailedLoginCount", 0);
            Logger.LogInformation($"User '{Input.Username}' logged in at {DateTime.Now}");
            RedirectManager.RedirectTo(IsDemoHost && roles.Contains(DemoAccess.Role) ? "/" : ReturnUrl);
        }
        else
        {
            // increment failed attempts
            failedAttempts++;
            HttpContext.Session.SetInt32("FailedLoginCount", failedAttempts);
            if (IsDemoHost && failedAttempts > 2)
            {
                _showCaptcha = true;
                if (_demoChallenge is null) RefreshDemoCaptcha();
                Input.CaptchaAnswer = "";
            }

            if (result.RequiresTwoFactor)
            {
                RedirectManager.RedirectTo(ClientRoutes.Accounts.LoginWith2Fa,
                    new Dictionary<string, object?> { ["returnUrl"] = ReturnUrl, ["rememberMe"] = Input.RememberMe });
            }
            else if (result.IsLockedOut)
            {
                Logger.LogWarning("User account locked out.");
                RedirectManager.RedirectTo(ClientRoutes.Accounts.Lockout);
            }
            else
            {
                _errorMessage = IsDemoHost && result.IsNotAllowed && Input.Username.StartsWith("demo_", StringComparison.OrdinalIgnoreCase)
                    ? DemoAccess.ExpiredMessage : "خطا: اطلاعات ورودی معتبر نیست";
            }
        }
    }

    private async Task<bool> VerifyCaptchaAsync(string captchaResponse)
    {
        try
        {
            var secretKey = Config["Turnstile:SecretKey"];
            var client = HttpClientFactory.CreateClient();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["secret"] = secretKey ?? "",
                ["response"] = captchaResponse,
                ["remoteip"] = HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""
            });

            var response = await client.PostAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify", content);
            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<TurnstileVerifyResponse>(json);
            return result?.success == true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error verifying Turnstile CAPTCHA");
            return false;
        }
    }

    private sealed class InputModel
    {
        [Display(Name = "نام کاربری")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Username { get; set; } = "";

        [Display(Name = "رمز عبور")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = ""; 

        [Display(Name = "مرا بخاطر بسپار")]
        public bool RememberMe { get; set; }

        public string? CaptchaId { get; set; }
        public string? CaptchaAnswer { get; set; }
        public PasskeyInputModel? Passkey { get; set; }
    }

    private sealed class PasskeyInputModel
    {
        public string? CredentialJson { get; set; }
        public string? Error { get; set; }
    }

    private sealed class TurnstileVerifyResponse
    {
        public bool success { get; set; }
        public string[]? error_codes { get; set; }
    }
}