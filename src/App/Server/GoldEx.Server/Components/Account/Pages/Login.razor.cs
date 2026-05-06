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

    [CascadingParameter] private HttpContext HttpContext { get; set; } = default!;
    [SupplyParameterFromForm] private InputModel Input { get; set; } = new();
    [SupplyParameterFromQuery] private string? ReturnUrl { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _siteKey = Config["Turnstile:SiteKey"] ?? throw new InvalidOperationException("Turnstile:SiteKey is missing");

        // Check failed attempts from session
        var failedAttempts = HttpContext.Session.GetInt32("FailedLoginCount") ?? 0;
        _showCaptcha = failedAttempts > 5;

        if (HttpMethods.IsGet(HttpContext.Request.Method))
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }
    }

    private async Task LoginUser()
    {
        var failedAttempts = HttpContext.Session.GetInt32("FailedLoginCount") ?? 0;

        //If failedAttempts > 5, require CAPTCHA
        if (_showCaptcha)
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
            var onlyAllowedRoles = roles.Count > 0 && roles.All(r => allowedRoles.Contains(r));
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
            RedirectManager.RedirectTo(ReturnUrl);
        }
        else
        {
            // increment failed attempts
            failedAttempts++;
            HttpContext.Session.SetInt32("FailedLoginCount", failedAttempts);

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
                _errorMessage = "خطا: اطلاعات ورودی معتبر نیست";
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