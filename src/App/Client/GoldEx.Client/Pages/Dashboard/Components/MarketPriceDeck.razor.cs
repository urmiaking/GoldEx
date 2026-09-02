using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GoldEx.Client.Pages.Dashboard.Components;

public partial class MarketPriceDeck
{
    [Inject] private IPriceStateService PriceStateService { get; set; } = default!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    private List<GetPriceResponse> _items = [];
    private string? _selectedCategory;
    private readonly ConcurrentDictionary<Guid, string> _flashingItems = new();

    private IEnumerable<IGrouping<string, GetPriceResponse>> GroupedItems =>
        _items.GroupBy(x => x.Type.GetDisplayName());

    private IEnumerable<GetPriceResponse> FilteredItems =>
        string.IsNullOrEmpty(_selectedCategory)
            ? _items
            : _items.Where(x => x.Type.GetDisplayName() == _selectedCategory);

    private IEnumerable<GetPriceResponse> LoopItems
    {
        get
        {
            var list = FilteredItems.ToList();
            if (!list.Any()) return list;
            return list.Concat(list).Concat(list);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        PriceStateService.OnPricesUpdated += OnPricesUpdated;
        PriceStateService.OnPriceBatchChanged += HandlePriceBatchChanged;
        await LoadPricesAsync();
        await base.OnInitializedAsync();
    }

    private async void OnPricesUpdated()
    {
        await InvokeAsync(async () =>
        {
            if (IsDisposed) return;
            await LoadPricesAsync();
            StateHasChanged();
        });
    }

    private async void HandlePriceBatchChanged(List<PriceChangedNotificationDto> updates)
    {
        if (!updates.Any()) return;

        await InvokeAsync(() =>
        {
            if (IsDisposed) return;

            foreach (var update in updates)
            {
                var flashClass = update.Direction switch
                {
                    PriceChangeDirection.Up => "flash-up",
                    PriceChangeDirection.Down => "flash-down",
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(flashClass))
                {
                    _flashingItems[update.Id] = flashClass;
                }
            }

            StateHasChanged();

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(2600);
                    if (IsDisposed) return;

                    foreach (var update in updates)
                    {
                        _flashingItems.TryRemove(update.Id, out _);
                    }

                    await InvokeAsync(() =>
                    {
                        if (IsDisposed) return;
                        StateHasChanged();
                    });
                }
                catch
                {
                }
            });
        });
    }

    private string GetFlashClass(Guid id) =>
        _flashingItems.TryGetValue(id, out var cls) ? cls : string.Empty;

    private async Task LoadPricesAsync()
    {
        await SendRequestAsync<IPriceStateService, List<GetPriceResponse>>(
            action: (s, ct) => s.GetListAsync(true, ct),
            afterSend: response => _items = response
        );
    }

    private void SelectCategory(string? category)
    {
        _selectedCategory = category;
    }

    public override async ValueTask DisposeAsync()
    {
        PriceStateService.OnPricesUpdated -= OnPricesUpdated;
        PriceStateService.OnPriceBatchChanged -= HandlePriceBatchChanged;
        await base.DisposeAsync();
    }

    public static double ExtractPercentChange(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return 0;
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
