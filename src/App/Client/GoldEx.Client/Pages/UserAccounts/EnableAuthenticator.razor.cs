using GoldEx.Client.Pages.UserAccounts.Components;
using GoldEx.Shared.DTOs.UserAccounts;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using MudBlazor;
using Net.Codecrete.QrCodeGenerator;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;

namespace GoldEx.Client.Pages.UserAccounts;

public partial class EnableAuthenticator
{
    private const string AuthenticatorUriFormat = "otpauth://totp/{0}?secret={1}&issuer={0}&digits=6";

    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("مدیریت حساب", href: ClientRoutes.UserAccounts.Index, icon: Icons.Material.Filled.Person),
        new("رمز دو مرحله ای", href: ClientRoutes.UserAccounts.TwoFactorAuthentication, icon: Icons.Material.Filled.Password),
        new("فعالسازی", href: ClientRoutes.UserAccounts.EnableAuthenticator, icon: Icons.Material.Filled.Check),
    ];

    private GetAuthenticatorKeyResponse? _authenticatorKey;
    private string? _svgGraphicsPath;
    private string? _code;
    private string? _formattedKey;

    protected override async Task OnInitializedAsync()
    {
        await LoadSharedKeyAndQrCodeUriAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadSharedKeyAndQrCodeUriAsync()
    { 
        _authenticatorKey = await SendRequestAsync<IUserAccountService, GetAuthenticatorKeyResponse>(
            action: (s, ct) => s.GetAuthenticatorKeyAsync(ct));

        _formattedKey = FormatKey(_authenticatorKey!.Key);

        var authenticatorUri = GenerateQrCodeUri(_authenticatorKey!.Key);
        var qr = QrCode.EncodeText(authenticatorUri, QrCode.Ecc.Medium);
        _svgGraphicsPath = qr.ToGraphicsPath();
    }

    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        var currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string unformattedKey)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            AuthenticatorUriFormat,
            UrlEncoder.Default.Encode("GoldEx"),
            unformattedKey);
    }

    private async Task OnEnable()
    {
        if (string.IsNullOrEmpty(_code))
            return;

        await SendRequestAsync<IUserAccountService, EnableTwoFactorAuthResponse>(
            action: (s, ct) => s.Enable2FaAsync(new EnableTwoFactorAuthRequest(_code), ct),
            afterSend: async response =>
            {
                var parameters = new DialogParameters<RecoveryCodeViewer>
                {
                    { x => x.RecoveryCodes, response.RecoveryCodes }
                };

                var dialog = await DialogService.ShowAsync<RecoveryCodeViewer>("کدهای بازیابی",
                    parameters,
                    new DialogOptions
                    {
                        CloseButton = false,
                        MaxWidth = MaxWidth.Small,
                        FullWidth = true,
                        BackdropClick = false
                    });

                await dialog.Result;

                Navigation.NavigateTo(ClientRoutes.UserAccounts.TwoFactorAuthentication);
            });
    }
}