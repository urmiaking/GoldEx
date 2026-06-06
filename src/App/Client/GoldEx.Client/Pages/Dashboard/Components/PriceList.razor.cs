using System.Globalization;
using System.Text.RegularExpressions;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Dashboard.Components;

public partial class PriceList
{
    [Inject] private IPriceStateService PriceStateService { get; set; } = default!;

    private List<GetPriceResponse> _items = [];

    protected override async Task OnInitializedAsync()
    {
        PriceStateService.OnPricesUpdated += OnPricesUpdated;
        await LoadSettingsAsync();
        await LoadPricesAsync();
        await base.OnInitializedAsync();
    }

    private async void OnPricesUpdated()
    {
        await InvokeAsync(async () =>
        {
            await LoadPricesAsync();
            StateHasChanged();
        });
    }

    private async Task LoadSettingsAsync()
    {
        await SendRequestAsync<ISettingService, GetSettingResponse?>(
            action: (s, ct) => s.GetAsync(ct),
            createScope: true
        );
    }

    private async Task LoadPricesAsync()
    {
        await SendRequestAsync<IPriceStateService, List<GetPriceResponse>>(
            action: (s, ct) => s.GetListAsync(true, ct),
            afterSend: response => _items = response
        );
    }

    public override async ValueTask DisposeAsync()
    {
        PriceStateService.OnPricesUpdated -= OnPricesUpdated;
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