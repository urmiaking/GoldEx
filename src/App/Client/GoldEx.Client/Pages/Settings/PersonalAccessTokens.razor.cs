using GoldEx.Client.Pages.Settings.Components.PersonalAccessTokens;
using GoldEx.Shared.DTOs.PersonalAccessTokens;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoldEx.Client.Pages.Settings;

public partial class PersonalAccessTokens
{
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, MaxWidth = MaxWidth.Small };
    private List<PersonalAccessTokenDto> _tokens = [];
    private bool _processing;

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await LoadTokensAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadTokensAsync()
    {
        _processing = true;

        await SendRequestAsync<IPersonalAccessTokenService, List<PersonalAccessTokenDto>>(
            action: (s, ct) => s.GetListAsync(ct),
            afterSend: response =>
            {
                _tokens = response ?? [];
                _processing = false;
                return Task.CompletedTask;
            });
    }

    private async Task OnCreateToken()
    {
        var dialog = await DialogService.ShowAsync<CreateTokenDialog>("ایجاد کلید دسترسی هوش مصنوعی", _dialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CreatePersonalAccessTokenResponse response })
        {
            await LoadTokensAsync();

            var revealParams = new DialogParameters<RevealTokenDialog>
            {
                { x => x.Token, response.RawToken },
                { x => x.Name, response.Name },
                { x => x.TokenPrefix, response.TokenPrefix }
            };

            await DialogService.ShowAsync<RevealTokenDialog>("کلید دسترسی با موفقیت صادر شد", revealParams, _dialogOptions);
        }
    }

    private async Task OnRevokeToken(PersonalAccessTokenDto token)
    {
        var confirm = await DialogService.ShowMessageBoxAsync(
            "تایید ابطال کلید",
            $"آیا از ابطال کلید «{token.Name}» مطمئن هستید؟ پس از ابطال، دستیارهای متصل با این کلید دیگر قادر به اتصال نخواهند بود.",
            yesText: "بله، باطل شود",
            cancelText: "انصراف");

        if (confirm == true)
        {
            await SendRequestAsync<IPersonalAccessTokenService>(
                action: (s, ct) => s.RevokeAsync(token.Id, ct),
                afterSend: () =>
                {
                    AddSuccessToast("کلید دسترسی با موفقیت باطل شد.");
                    return LoadTokensAsync();
                });
        }
    }

    private async Task OnDeleteToken(PersonalAccessTokenDto token)
    {
        var confirm = await DialogService.ShowMessageBoxAsync(
            "تایید حذف کلید",
            $"آیا از حذف کلید «{token.Name}» مطمئن هستید؟",
            yesText: "بله، حذف شود",
            cancelText: "انصراف");

        if (confirm == true)
        {
            await SendRequestAsync<IPersonalAccessTokenService>(
                action: (s, ct) => s.DeleteAsync(token.Id, ct),
                afterSend: () =>
                {
                    AddSuccessToast("کلید دسترسی با موفقیت حذف شد.");
                    return LoadTokensAsync();
                });
        }
    }

    private string GetMcpEndpointUrl()
    {
        var baseUri = Navigation.BaseUri.TrimEnd('/');
        return $"{baseUri}{ApiRoutes.Mcp.Base}";
    }

    private string GetCursorConfigJson()
    {
        var url = GetMcpEndpointUrl();
        return $$"""
        {
          "mcpServers": {
            "goldex": {
              "url": "{{url}}",
              "headers": {
                "Authorization": "Bearer YOUR_PAT_KEY"
              }
            }
          }
        }
        """;
    }

    private string GetClaudeConfigJson()
    {
        var url = GetMcpEndpointUrl();
        return $$"""
        {
          "mcpServers": {
            "goldex": {
              "url": "{{url}}",
              "headers": {
                "Authorization": "Bearer YOUR_PAT_KEY"
              }
            }
          }
        }
        """;
    }

    private async Task CopyToClipboard(string text)
    {
        try
        {
            await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
            AddSuccessToast("متن با موفقیت در کلیپ‌بورد کپی شد.");
        }
        catch
        {
            AddErrorToast("خطا در کپی خودکار. لطفاً متن را دستی کپی فرمایید.");
        }
    }
}
