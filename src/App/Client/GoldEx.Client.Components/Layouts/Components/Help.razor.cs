using GoldEx.Client.Components.Services;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Components.Layouts.Components;

public partial class Help
{
    private bool _hasHelp;

    [Inject] private HelpContext HelpContext { get; set; } = default!;

    protected override void OnInitialized()
    {
        HelpContext.OnChanged += OnHelpContextChanged;
    }

    private async void OnHelpContextChanged()
    {
        await RefreshAsync();
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnInitializedAsync()
    {
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        _hasHelp = !string.IsNullOrEmpty(HelpContext.Slug)
                   && await HasHelpAsync();
    }

    public override ValueTask DisposeAsync()
    {
        HelpContext.OnChanged -= OnHelpContextChanged;
        return base.DisposeAsync();
    }

    private async Task<bool> HasHelpAsync()
    {
        var result = false;

        if (string.IsNullOrEmpty(HelpContext.Slug))
        {
            result = false;
        }
        else
        {
            await SendRequestAsync<IBlogPostService, bool>(
                action: (s, ct) => s.ExistsAsync(HelpContext.Slug, ct),
                afterSend: response => result = response);
        }

        return result;
    }

    private async Task OpenDialog()
    {
        if (string.IsNullOrEmpty(HelpContext.Slug))
            return;

        var parameters = new DialogParameters<HelpContent>
        {
            { x => x.Slug, HelpContext.Slug }
        };

        await DialogService.ShowAsync<HelpContent>("راهنما", parameters, new DialogOptions { FullWidth = true, FullScreen = true, CloseButton = true});
    }
}