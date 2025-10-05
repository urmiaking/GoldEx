using System.Globalization;
using System.Text.RegularExpressions;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Services.Abstractions;

namespace GoldEx.Client.Pages.Dashboard.Components;

public partial class PriceList
{
    private List<GetPriceResponse> _items = [];
    private Timer? _timer;
    private TimeSpan _updateInterval = TimeSpan.FromSeconds(30);

    protected override async Task OnInitializedAsync()
    {
        await LoadSettingsAsync();
        await LoadPricesAsync();
        await StartTimer();
        await base.OnInitializedAsync();
    }

    private async Task LoadSettingsAsync()
    {
        await SendRequestAsync<ISettingService, GetSettingResponse?>(
            action: (s, ct) => s.GetAsync(ct),
            afterSend: response =>
            {
                if (response?.PriceUpdateInterval > TimeSpan.Zero)
                {
                    _updateInterval = response.PriceUpdateInterval;
                }
            },
            createScope: true
        );
    }

    private async Task LoadPricesAsync()
    {
        await SendRequestAsync<IPriceService, List<GetPriceResponse>>(
            action: (s, ct) => s.GetListAsync(true, ct),
            afterSend: response => _items = response,
            createScope: true
        );
    }

    private Task StartTimer()
    {
        _timer = new Timer(
            TimerCallback,
            null,
            _updateInterval,
            _updateInterval
        );

        return Task.CompletedTask;
    }

    private async void TimerCallback(object? state)
    {
        await LoadPricesAsync();
        StateHasChanged();
    }

    public override async ValueTask DisposeAsync()
    {
        if (_timer is not null)
            await _timer.DisposeAsync();

        await base.DisposeAsync();
    }

    public static double ExtractPercentChange(string input)
    {
        var match = Regex.Match(input, @"([-+]?\d+(?:[.,]\d+)?)\s*%");
        if (match.Success && double.TryParse(match.Groups[1].Value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var result))
        {
            return result;
        }
        return 0;
    }
}