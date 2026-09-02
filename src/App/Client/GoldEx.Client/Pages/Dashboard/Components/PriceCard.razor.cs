using System.Globalization;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Dashboard.Components;

public partial class PriceCard
{
    [Inject] private IPriceStateService PriceStateService { get; set; } = default!;

    [Parameter] public Guid? Id { get; set; }
    [Parameter] public string? ImageUrl { get; set; }
    [Parameter] public string? Title { get; set; }
    [Parameter] public DateTime? DateTime { get; set; }
    [Parameter] public string? Price { get; set; }
    [Parameter] public string? PriceUnit { get; set; }
    [Parameter] public double ChangePercent { get; set; }

    private string _flashClass = string.Empty;
    private string? _previousPrice;
    private CancellationTokenSource? _flashCts;

    private string ChangeIcon => ChangePercent < 0 ? Icons.Material.Filled.ArrowDropDown : Icons.Material.Filled.ArrowDropUp;
    private Color ChangeColor => ChangePercent < 0 ? Color.Error : Color.Success;

    protected override void OnInitialized()
    {
        PriceStateService.OnPriceChanged += HandlePriceChanged;
        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        if (!string.IsNullOrEmpty(_previousPrice) && !string.IsNullOrEmpty(Price) && _previousPrice != Price)
        {
            if (TryParsePrice(Price, out var newPrice) && TryParsePrice(_previousPrice, out var oldPrice))
            {
                if (newPrice > oldPrice)
                {
                    TriggerFlash(PriceChangeDirection.Up);
                }
                else if (newPrice < oldPrice)
                {
                    TriggerFlash(PriceChangeDirection.Down);
                }
            }
        }

        _previousPrice = Price;
        base.OnParametersSet();
    }

    private async void HandlePriceChanged(PriceChangedNotificationDto update)
    {
        if ((Id.HasValue && update.Id == Id.Value) ||
            (!string.IsNullOrEmpty(Title) && string.Equals(update.Title, Title, StringComparison.OrdinalIgnoreCase)))
        {
            await InvokeAsync(() =>
            {
                if (IsDisposed) return;

                Price = update.Value;
                PriceUnit = update.Unit;
                DateTime = update.LastUpdate;
                ChangePercent = update.ChangePercent;
                _previousPrice = update.Value;

                TriggerFlash(update.Direction);
            });
        }
    }

    private void TriggerFlash(PriceChangeDirection direction)
    {
        _flashClass = direction switch
        {
            PriceChangeDirection.Up => "price-card-flash-up",
            PriceChangeDirection.Down => "price-card-flash-down",
            _ => string.Empty
        };

        StateHasChanged();

        if (!string.IsNullOrEmpty(_flashClass))
        {
            _flashCts?.Cancel();
            _flashCts?.Dispose();
            _flashCts = new CancellationTokenSource();
            var token = _flashCts.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(2600, token);
                    if (!token.IsCancellationRequested)
                    {
                        await InvokeAsync(() =>
                        {
                            if (IsDisposed) return;
                            _flashClass = string.Empty;
                            StateHasChanged();
                        });
                    }
                }
                catch (OperationCanceledException)
                {
                }
            }, token);
        }
    }

    private static bool TryParsePrice(string? priceStr, out decimal price)
    {
        price = 0;
        if (string.IsNullOrWhiteSpace(priceStr)) return false;
        var cleaned = priceStr.Replace(",", "").Replace("٬", "").Trim();
        return decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out price);
    }

    public override async ValueTask DisposeAsync()
    {
        PriceStateService.OnPriceChanged -= HandlePriceChanged;
        _flashCts?.Cancel();
        _flashCts?.Dispose();
        await base.DisposeAsync();
    }
}
