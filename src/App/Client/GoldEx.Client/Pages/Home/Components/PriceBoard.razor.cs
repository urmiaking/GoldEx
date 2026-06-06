using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GoldEx.Client.Pages.Home.Components;

public partial class PriceBoard
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public string ContainerClass { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 0;
    [Parameter] public bool ShowTitle { get; set; }

    private string ItemsKey => $"PriceBoard:Items:{Class}:{ContainerClass}";

    [Inject] private IPriceStateService PriceStateService { get; set; } = default!;

    private IEnumerable<GetPriceResponse>? _items;

    protected override async Task OnInitializedAsync()
    {
        RestorePersistedState();
        PriceStateService.OnPricesUpdated += OnPricesUpdated;
        if (_items is null)
        {
            await LoadPricesAsync();
        }
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

    protected override Task OnPersisting()
    {
        if (_items is not null)
            PersistStateAsJson(ItemsKey, _items);

        return base.OnPersisting();
    }

    private void RestorePersistedState()
    {
        if (RestoreStateFromJson(ItemsKey, out List<GetPriceResponse>? persistedItems) && persistedItems is not null)
        {
            _items = persistedItems;
        }
    }

    private async Task LoadPricesAsync()
    {
        await SendRequestAsync<IPriceStateService, List<GetPriceResponse>>(
            action: (s, ct) => s.GetListAsync(null, ct),
            afterSend: response => _items = response.OrderBy(x => x.Type.GetDisplayOrder())
        );
    }

    public override async ValueTask DisposeAsync()
    {
        PriceStateService.OnPricesUpdated -= OnPricesUpdated;
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
