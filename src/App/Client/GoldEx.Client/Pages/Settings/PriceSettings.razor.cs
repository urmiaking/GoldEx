using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Services.Abstractions;

namespace GoldEx.Client.Pages.Settings;

public partial class PriceSettings
{
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
}