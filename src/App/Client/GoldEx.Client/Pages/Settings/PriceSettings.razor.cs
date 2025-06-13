using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Services;

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
            action: (s, ct) => s.SetStatusAsync(id, request, ct));

        AddSuccessToast("تغییرات با موفقیت اعمال شد");
    }

    private void CloseAlert(bool value)
    {
        if (value)
        {
            _showAlert = false;
        }
    }
}