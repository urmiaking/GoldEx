using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Text.RegularExpressions;
using GoldEx.Sdk.Common.Definitions;

namespace GoldEx.Client.Pages.Home.Components;

public partial class PriceBoard
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public string ContainerClass { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 0;
    [Parameter] public bool ShowTitle { get; set; }

    private IEnumerable<GetPriceResponse>? _items;
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
            action: (s, ct) => s.GetListAsync(null, ct),
            afterSend: response => _items = response.OrderBy(x => x.Type.GetDisplayOrder()),
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
        await InvokeAsync(StateHasChanged);
    }

    public override async ValueTask DisposeAsync()
    {
        if (_timer is not null)
            await _timer.DisposeAsync();

        await base.DisposeAsync();
    }

    private static double ExtractPercentChange(string input)
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
