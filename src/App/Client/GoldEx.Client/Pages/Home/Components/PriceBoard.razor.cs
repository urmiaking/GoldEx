using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Home.Components;

public partial class PriceBoard
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 0;

    private readonly TableGroupDefinition<GetPriceResponse> _groupDefinition = new()
    {
        GroupName = "گروه",
        Indentation = false,
        Expandable = true,
        Selector = e => e.Type.GetDisplayName()
    };

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
            action: (s, ct) => s.GetListAsync(ct),
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

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}
