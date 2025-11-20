using GoldEx.Client.Pages.InventoryEntry.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.InventoryEntry.Components;

public partial class InventoryEntryContainer
{
    private readonly InventoryEntryVm _model = new();
    private GetPriceUnitResponse? _priceUnit;
    private decimal? _gramPrice;
    private readonly string _jsVersion = new Random().Next(1, 1000).ToString();

    // Processing State
    private bool _processing;
    private int _totalItems;
    private int _currentItemCount;
    private string _progressMessage = "در حال آماده‌سازی...";
    private CancellationTokenSource? _simCts;

    private int GetPercentage() =>
        _totalItems == 0 ? 0 : (int)((_currentItemCount / (double)_totalItems) * 100);

    [Parameter] public string? Class { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadDefaultPriceUnitAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadDefaultPriceUnitAsync()
    {
        await SendRequestAsync<IPriceUnitService, GetPriceUnitResponse?>(
            action: (s, ct) => s.GetDefaultAsync(ct),
            afterSend: response => _priceUnit = response);
    }

    private async Task OnSubmitAsync()
    {
        if (_priceUnit is null)
        {
            AddErrorToast("واحد قیمت پیش فرض بارگذاری نشده است.");
            return;
        }

        try
        {
            _model.ToRequest(_gramPrice ?? 0);
        }
        catch(Exception ex)
        {
            AddErrorToast(ex.Message);
            return;
        }

        var parts = new List<string>();

        if (_model.ProductItems.Count > 0)
            parts.Add($"{_model.ProductItems.Count} جنس");

        if (_model.CoinItems.Count > 0)
            parts.Add($"{_model.CoinItems.Count} سکه");

        if (_model.CurrencyItems.Count > 0)
            parts.Add($"{_model.CurrencyItems.Count} ارز");

        var promptDescription = $"آیا از ثبت ورود موجودی {string.Join("، ", parts)} اطمینان دارید؟";

        var result = await DialogService.ShowMessageBox(
            "تأیید نهایی",
            promptDescription,
            yesText: "بله", cancelText: "لغو");

        if (result is true)
        {
            _processing = true;
            _totalItems = _model.ProductItems.Count + _model.CoinItems.Count + _model.CurrencyItems.Count;

            if (_totalItems == 0) 
                _totalItems = 1;

            _currentItemCount = 0;

            _simCts = new CancellationTokenSource();
            var simulationTask = SimulateUploadProgressAsync(_simCts.Token);

            var request = _model.ToRequest(_gramPrice ?? 0);

            await SendRequestAsync<IInventoryEntryService>(
                action: (s, ct) => s.CreateAsync(request, CancellationToken.None),
                afterSend: async () =>
                {
                    await _simCts.CancelAsync();

                    try { await simulationTask; } catch (OperationCanceledException) { }

                    _currentItemCount = _totalItems;
                    _progressMessage = "عملیات با موفقیت انجام شد";
                    StateHasChanged();

                    await Task.Delay(500);

                    _processing = false;
                    AddSuccessToast("ورود موجودی با موفقیت انجام شد.");
                    Navigation.NavigateTo(ClientRoutes.InventoryStocks.List);
                },
                onFailure: () =>
                {
                    _simCts.Cancel();
                    _processing = false;
                    return Task.CompletedTask;
                });
        }
    }

    private void OnGramPriceChanged(decimal gramPrice)
    {
        _gramPrice = gramPrice;
    }

    private async Task SimulateUploadProgressAsync(CancellationToken ct)
    {
        try
        {
            // We will only update the UI if enough time has passed
            var lastUpdate = DateTime.MinValue;

            for (int i = 1; i <= _totalItems; i++)
            {
                ct.ThrowIfCancellationRequested();

                _currentItemCount = i;

                // Calculate percentage
                var percentage = GetPercentage();

                // Logic to determine message
                if (percentage < 20) _progressMessage = "بررسی صحت اطلاعات...";
                else if (percentage < 50) _progressMessage = "رمزنگاری و آماده‌سازی...";
                else if (percentage < 80) _progressMessage = "همگام‌سازی با پایگاه داده...";
                else _progressMessage = "ثبت نهایی...";

                // CRITICAL CHANGE: 
                // Only call StateHasChanged if 50ms have passed OR it's the last item.
                // This allows the loop to run at 10ms speed, but updates UI at 20fps (smooth).
                if ((DateTime.Now - lastUpdate).TotalMilliseconds > 50 || i == _totalItems)
                {
                    StateHasChanged();
                    lastUpdate = DateTime.Now;
                }

                // Now this 10ms delay will actually happen because we aren't waiting for the UI render every time
                await Task.Delay(5, ct);
            }

            // Wait here until the API cancels this task
            _progressMessage = "در حال دریافت تأییدیه نهایی...";
            StateHasChanged();
            await Task.Delay(Timeout.Infinite, ct);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
    }
}