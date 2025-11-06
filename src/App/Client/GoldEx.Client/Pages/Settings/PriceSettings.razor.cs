using GoldEx.Client.Pages.Settings.Components.Prices;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Services.Abstractions;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings;

public partial class PriceSettings
{
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false };
    private List<GetPriceSettingResponse> _priceSettings = [];
    private bool _showAlert = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadPriceSettingsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadPriceSettingsAsync()
    {
        await SendRequestAsync<IPriceService, List<GetPriceSettingResponse>>(
            action: (s, ct) => s.GetSettingsAsync(ct),
            afterSend: response =>
            {
                _priceSettings = response;
            });
    }

    private async Task OnPriceItemSelectionChanged(Guid id, bool isActive)
    {
        var request = new UpdatePriceStatusRequest(isActive);

        await SendRequestAsync<IPriceService>(
            action: (s, ct) => s.SetStatusAsync(id, request, ct),
            afterSend: () =>
            {
                AddSuccessToast("تغییرات با موفقیت اعمال شد");
                return Task.CompletedTask;
            });
    }

    private void CloseAlert(bool value)
    {
        if (value)
        {
            _showAlert = false;
        }
    }

    private async Task OnPinToggleAsync(Guid id, bool currentPinState)
    {
        await SendRequestAsync<IPriceService>(
            action: (s, ct) => s.SetPinnedAsync(id, !currentPinState, ct),
            afterSend: async () =>
            {
                await LoadPriceSettingsAsync();
                AddSuccessToast(currentPinState ? "سنجاق برداشته شد" : "سنجاق شد");
            });
    }

    private async Task OnEditPrice(PriceSettingDto model)
    {
        var parameters = new DialogParameters<Editor>
        {
            { x => x.Price, model }
        };

        var dialog = await DialogService.ShowAsync<Editor>($"تنظیمات نرخ {model.Title}", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("ویرایش با موفقیت انجام شد");
            await LoadPriceSettingsAsync();
            StateHasChanged();
        }
    }
}